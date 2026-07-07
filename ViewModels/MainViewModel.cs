using Autodesk.Revit.DB;
using ParameterManager.Common;
using ParameterManager.Contracts;
using ParameterManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ParameterManager.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IFamilyTypeRepository _familyTypeRepository;
        private readonly IElementOptionRepository _elementOptionRepository;
        private readonly ISelectionService _selectionService;
        private readonly IParameterService _parameterService;

        private bool _activeViewOnly = true;
        private string _selectionScopeText;

        private string _statusText;
        private string _selectedCategorySummary;
        private bool _isCategoryPopupOpen;
        private string _currentActionName;

        private IList<MaterialItem> _materials;

        public ObservableCollection<CategoryItem> Categories { get; set; }

        public ObservableCollection<FamilyTreeNode> FamilyTree { get; set; }

        public ObservableCollection<ParameterTreeNode> TypeParameterTree { get; set; }

        public ObservableCollection<ParameterTreeNode> InstanceParameterTree { get; set; }
        public bool IsActiveViewScope
        {
            get => ActiveViewOnly;
            set
            {
                if (value)
                {
                    SetSelectionScope(true);
                }
            }
        }

        public bool IsAllProjectScope
        {
            get => !ActiveViewOnly;
            set
            {
                if (value)
                {
                    SetSelectionScope(false);
                }
            }
        }
        public RelayCommand AssignCommand { get; set; }

        public RelayCommand CancelCommand { get; set; }
        public RelayCommand UseActiveViewCommand { get; set; }

        public RelayCommand UseAllProjectCommand { get; set; }

        public RelayCommand SelectElementsCommand { get; set; }

        public RelayCommand DeselectElementsCommand { get; set; }

        public RelayCommand HideElementsCommand { get; set; }

        public RelayCommand UnhideElementsCommand { get; set; }

        public RelayCommand OpenImagePickerCommand { get; set; }

        public event Action RequestClose;

        public string SelectedCategorySummary
        {
            get => _selectedCategorySummary;
            set
            {
                _selectedCategorySummary = value;
                OnPropertyChanged();
            }
        }

        public bool IsCategoryPopupOpen
        {
            get => _isCategoryPopupOpen;
            set
            {
                _isCategoryPopupOpen = value;
                OnPropertyChanged();
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }
        public bool ActiveViewOnly
        {
            get => _activeViewOnly;
            set
            {
                if (_activeViewOnly == value)
                    return;

                _activeViewOnly = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsActiveViewScope));
                OnPropertyChanged(nameof(IsAllProjectScope));

                SelectionScopeText = _activeViewOnly
                    ? "Scope: Active View"
                    : "Scope: All Project";
            }
        }

        public string SelectionScopeText
        {
            get => _selectionScopeText;
            set
            {
                _selectionScopeText = value;
                OnPropertyChanged();
            }
        }
        public string CurrentActionName
        {
            get => _currentActionName;
            set
            {
                if (_currentActionName == value)
                    return;

                _currentActionName = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSelectActionActive));
                OnPropertyChanged(nameof(IsDeselectActionActive));
                OnPropertyChanged(nameof(IsHideActionActive));
                OnPropertyChanged(nameof(IsUnhideActionActive));
            }
        }

        public bool IsSelectActionActive => CurrentActionName == "Select";

        public bool IsDeselectActionActive => CurrentActionName == "Deselect";

        public bool IsHideActionActive => CurrentActionName == "Hide";

        public bool IsUnhideActionActive => CurrentActionName == "Unhide";

        public MainViewModel(
    IFamilyTypeRepository familyTypeRepository,
    IElementOptionRepository elementOptionRepository,
    ISelectionService selectionService,
    IParameterService parameterService)
        {
            _familyTypeRepository = familyTypeRepository;
            _elementOptionRepository = elementOptionRepository;
            _selectionService = selectionService;
            _parameterService = parameterService;

            Categories = new ObservableCollection<CategoryItem>();
            FamilyTree = new ObservableCollection<FamilyTreeNode>();
            TypeParameterTree = new ObservableCollection<ParameterTreeNode>();
            InstanceParameterTree = new ObservableCollection<ParameterTreeNode>();

            AssignCommand = new RelayCommand(x => AssignEditedParameters());
            CancelCommand = new RelayCommand(x => RequestClose?.Invoke());

            UseActiveViewCommand = new RelayCommand(x => SetSelectionScope(true));
            UseAllProjectCommand = new RelayCommand(x => SetSelectionScope(false));
            SelectElementsCommand = new RelayCommand(x => SelectElementsInRevit());
            DeselectElementsCommand = new RelayCommand(x => DeselectElementsInRevit());
            HideElementsCommand = new RelayCommand(x => HideElementsInActiveView());
            UnhideElementsCommand = new RelayCommand(x => UnhideElementsInActiveView());
            OpenImagePickerCommand = new RelayCommand(x => OpenImagePicker(x as ParameterTreeNode));

            SelectionScopeText = "Scope: Active View";

            LoadCategories();
        }

        private void LoadCategories()
        {
            Categories.Clear();

            foreach (CategoryItem item in _familyTypeRepository.GetCategories())
            {
                item.SelectionChanged = OnCategorySelectionChanged;
                Categories.Add(item);
            }

            UpdateCategorySummary();
            StatusText = "Select one or more categories.";
        }

        private void OnCategorySelectionChanged()
        {
            UpdateCategorySummary();
            LoadFamilyTree();
        }

        private void UpdateCategorySummary()
        {
            List<string> checkedNames = Categories
                .Where(x => x.IsChecked)
                .Select(x => x.Name)
                .ToList();

            if (checkedNames.Count == 0)
            {
                SelectedCategorySummary = "Select categories...";
            }
            else if (checkedNames.Count == 1)
            {
                SelectedCategorySummary = checkedNames[0];
            }
            else
            {
                SelectedCategorySummary = $"{checkedNames.Count} categories selected";
            }
        }

        private void LoadFamilyTree()
        {
            FamilyTree.Clear();
            TypeParameterTree.Clear();
            InstanceParameterTree.Clear();

            List<CategoryItem> selectedCategories = Categories
                .Where(x => x.IsChecked)
                .ToList();

            if (selectedCategories.Count == 0)
            {
                StatusText = "Select one or more categories.";
                return;
            }

            IList<ElementType> types =
    _familyTypeRepository.GetTypesByCategoryIds(
        selectedCategories.Select(x => x.CategoryId),
        ActiveViewOnly);

            var categoryGroups = types
                .GroupBy(x => x.Category.Id.Value)
                .OrderBy(x => x.First().Category.Name);

            foreach (var categoryGroup in categoryGroups)
            {
                string categoryName = categoryGroup.First().Category.Name;

                FamilyTreeNode categoryNode = new FamilyTreeNode
                {
                    Name = categoryName,
                    CategoryId = categoryGroup.Key,
                    Kind = FamilyTreeNodeKind.Category,
                    IsExpanded = true,
                    SelectionChanged = OnTypeSelectionChanged
                };

                var familyGroups = categoryGroup
                    .GroupBy(x => GetFamilyName(x))
                    .OrderBy(x => x.Key);

                foreach (var familyGroup in familyGroups)
                {
                    FamilyTreeNode familyNode = new FamilyTreeNode
                    {
                        Name = familyGroup.Key,
                        FamilyName = familyGroup.Key,
                        CategoryId = categoryGroup.Key,
                        Kind = FamilyTreeNodeKind.Family,
                        Parent = categoryNode,
                        IsExpanded = false,
                        SelectionChanged = OnTypeSelectionChanged
                    };

                    foreach (ElementType type in familyGroup.OrderBy(x => x.Name))
                    {
                        FamilyTreeNode typeNode = new FamilyTreeNode
                        {
                            Name = type.Name,
                            FamilyName = familyGroup.Key,
                            CategoryId = categoryGroup.Key,
                            TypeIdValue = type.Id.Value,
                            Kind = FamilyTreeNodeKind.Type,
                            Parent = familyNode,
                            SelectionChanged = OnTypeSelectionChanged
                        };

                        familyNode.Children.Add(typeNode);
                    }

                    categoryNode.Children.Add(familyNode);
                }

                FamilyTree.Add(categoryNode);
            }

            StatusText = $"Loaded {FamilyTree.Count} categories. Check one or more types to view parameters.";
        }

        private void OnTypeSelectionChanged()
        {
            LoadParameterTreeFromSelectedTypes();
        }

        private void LoadParameterTreeFromSelectedTypes()
        {
            TypeParameterTree.Clear();
            InstanceParameterTree.Clear();

            IList<ElementType> selectedTypes = GetSelectedElementTypes();

            if (selectedTypes.Count == 0)
            {
                StatusText = "Select at least one family type from the left tree.";
                return;
            }

            IList<Element> selectedInstances = GetSelectedInstances();

            IList<ParameterDescriptor> descriptors =
                _parameterService.GetCommonEditableParameters(
                    selectedTypes,
                    selectedInstances);

            IList<ParameterDescriptor> typeDescriptors = descriptors
                .Where(x => x.Scope == ParameterScope.Type)
                .ToList();

            IList<ParameterDescriptor> instanceDescriptors = descriptors
                .Where(x => x.Scope == ParameterScope.Instance)
                .ToList();

            BuildParameterTree(
                typeDescriptors,
                TypeParameterTree);

            BuildParameterTree(
                instanceDescriptors,
                InstanceParameterTree);

            StatusText =
    $"Loaded {typeDescriptors.Count} type parameters and {instanceDescriptors.Count} instance parameters from {selectedTypes.Count} type(s), {selectedInstances.Count} instance(s).";
        }
        private void BuildParameterTree(
    IList<ParameterDescriptor> descriptors,
    ObservableCollection<ParameterTreeNode> targetTree)
        {
            targetTree.Clear();

            var groupByRevitGroup = descriptors
                .GroupBy(x => x.GroupName)
                .OrderBy(x => x.Key);

            foreach (var revitGroup in groupByRevitGroup)
            {
                ParameterTreeNode groupNode = new ParameterTreeNode
                {
                    Member = revitGroup.Key,
                    Value = string.Empty,
                    IsGroup = true,
                    CanEdit = false,
                    IsExpanded = true
                };

                foreach (ParameterDescriptor descriptor in revitGroup.OrderBy(x => x.ParameterName))
                {
                    ParameterTreeNode parameterNode = CreateParameterNode(descriptor);
                    groupNode.Children.Add(parameterNode);
                }

                targetTree.Add(groupNode);
            }
        }
        private ParameterTreeNode CreateParameterNode(ParameterDescriptor descriptor)
        {
            ParameterTreeNode node = new ParameterTreeNode
            {
                Member = descriptor.ParameterName,
                ParameterName = descriptor.ParameterName,
                ParameterId = descriptor.ParameterId,
                GroupName = descriptor.GroupName,
                ParameterSource = descriptor.SourceWithScope,
                StorageType = descriptor.StorageType,
                Scope = descriptor.Scope,
                EditorKind = descriptor.EditorKind,
                ElementOptionKind = descriptor.ElementOptionKind,
                CanEdit = true,
                IsGroup = false,
                ToolTipText =
                    $"Parameter: {descriptor.ParameterName}\n" +
                    $"Group: {descriptor.GroupName}\n" +
                    $"Source: {descriptor.SourceWithScope}\n" +
                    $"Storage: {descriptor.StorageType}"
            };

            if (descriptor.EditorKind == ParameterEditorKind.BooleanCheckBox)
            {
                node.SetInitialBoolean(descriptor.CommonBooleanValue);
                return node;
            }

            if (descriptor.EditorKind == ParameterEditorKind.ElementCombo ||
                descriptor.EditorKind == ParameterEditorKind.ImagePicker)
            {
                IList<ElementOptionItem> options =
                    _elementOptionRepository.GetOptions(descriptor.ElementOptionKind);

                foreach (ElementOptionItem option in options)
                {
                    node.ElementOptions.Add(option);
                }

                ElementOptionItem selectedOption =
                    FindElementOption(descriptor, options);

                node.SetInitialElementOption(selectedOption);
                return node;
            }

            node.SetInitialValue(descriptor.DisplayValue);
            return node;
        }

        private MaterialItem FindMaterialOption(ParameterDescriptor descriptor)
        {
            if (descriptor.DisplayValue == ParameterConstants.VariesText)
            {
                return _materials.FirstOrDefault(x => x.IsPlaceholder);
            }

            if (descriptor.CommonElementIdValue.HasValue)
            {
                MaterialItem byId = _materials.FirstOrDefault(
                    x => x.MaterialId == descriptor.CommonElementIdValue.Value);

                if (byId != null)
                    return byId;
            }

            MaterialItem byName = _materials.FirstOrDefault(
                x => x.Name == descriptor.DisplayValue);

            return byName ?? _materials.FirstOrDefault(x => x.IsPlaceholder);
        }

        private void AssignEditedParameters()
        {
            IList<ElementType> selectedTypes = GetSelectedElementTypes();
            IList<Element> selectedInstances = GetSelectedInstances();

            if (selectedTypes.Count == 0)
            {
                MessageBox.Show("No family type has been selected.");
                return;
            }

            IList<ParameterTreeNode> editedParameters =
     FlattenParameterTree(TypeParameterTree)
         .Concat(FlattenParameterTree(InstanceParameterTree))
         .Where(x => x.CanEdit)
         .Where(x => x.IsEdited)
         .Where(x => x.Value != ParameterConstants.VariesText)
         .ToList();

            if (editedParameters.Count == 0)
            {
                MessageBox.Show("No parameters have been edited.");
                return;
            }

            AssignResult result =
                _parameterService.AssignParameters(
                    selectedTypes,
                    selectedInstances,
                    editedParameters);

            if (result.HasErrors)
            {
                MessageBox.Show(
                    string.Join(Environment.NewLine, result.Errors.Take(30)),
                    "Some parameters could not be assigned");
            }
            else
            {
                MessageBox.Show("Parameter assignment successful.");
            }

            StatusText = $"Assigned {result.SuccessCount} values.";
            LoadParameterTreeFromSelectedTypes();
        }

        private IList<ElementType> GetSelectedElementTypes()
        {
            IEnumerable<long> selectedTypeIds = FlattenFamilyTree(FamilyTree)
                .Where(x => x.Kind == FamilyTreeNodeKind.Type)
                .Where(x => x.IsChecked == true)
                .Select(x => x.TypeIdValue);

            return _familyTypeRepository.GetTypesByIds(selectedTypeIds);
        }

        private IList<Element> GetSelectedInstances()
        {
            IEnumerable<long> selectedTypeIds = FlattenFamilyTree(FamilyTree)
                .Where(x => x.Kind == FamilyTreeNodeKind.Type)
                .Where(x => x.IsChecked == true)
                .Select(x => x.TypeIdValue);

            return _familyTypeRepository.GetInstancesByTypeIds(
                selectedTypeIds,
                ActiveViewOnly);
        }

        private IEnumerable<FamilyTreeNode> FlattenFamilyTree(
            IEnumerable<FamilyTreeNode> nodes)
        {
            foreach (FamilyTreeNode node in nodes)
            {
                yield return node;

                foreach (FamilyTreeNode child in FlattenFamilyTree(node.Children))
                {
                    yield return child;
                }
            }
        }

        private IEnumerable<ParameterTreeNode> FlattenParameterTree(
            IEnumerable<ParameterTreeNode> nodes)
        {
            foreach (ParameterTreeNode node in nodes)
            {
                yield return node;

                foreach (ParameterTreeNode child in FlattenParameterTree(node.Children))
                {
                    yield return child;
                }
            }
        }

        private string GetFamilyName(ElementType type)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(type.FamilyName))
                    return type.FamilyName;
            }
            catch
            {
            }

            return "System Family";
        }
        private void SetSelectionScope(bool activeViewOnly)
        {
            ActiveViewOnly = activeViewOnly;
            LoadFamilyTree();
        }

        private void SelectElementsInRevit()
        {
            IList<Element> selectedInstances = GetSelectedInstances();

            _selectionService.SelectElements(selectedInstances);

            CurrentActionName = "Select";
            StatusText = $"Selected {selectedInstances.Count} element(s) in Revit.";
        }

        private void DeselectElementsInRevit()
        {
            _selectionService.DeselectElements();

            CurrentActionName = "Deselect";
            StatusText = "Revit selection has been cleared.";
        }

        private void HideElementsInActiveView()
        {
            IList<Element> selectedInstances = GetSelectedInstances();

            int count = _selectionService.HideElementsInActiveView(selectedInstances);

            CurrentActionName = "Hide";
            StatusText = $"Hidden {count} element(s) in the active view.";
        }

        private void UnhideElementsInActiveView()
        {
            IList<Element> selectedInstances = GetSelectedInstances();

            int count = _selectionService.UnhideElementsInActiveView(selectedInstances);

            CurrentActionName = "Unhide";
            StatusText = $"Unhidden {count} element(s) in the active view.";
        }

        private void OpenImagePicker(ParameterTreeNode node)
        {
            if (node == null || !node.IsImagePickerEditor)
                return;

            Views.ImagePickerWindow picker =
                new Views.ImagePickerWindow(
                    node.ElementOptions.ToList(),
                    node.SelectedElementOption);

            bool? result = picker.ShowDialog();

            if (result == true && picker.SelectedOption != null)
            {
                node.SelectedElementOption = picker.SelectedOption;
            }
        }
        private ElementOptionItem FindElementOption(
    ParameterDescriptor descriptor,
    IList<ElementOptionItem> options)
        {
            if (descriptor.DisplayValue == ParameterConstants.VariesText)
            {
                return options.FirstOrDefault(x => x.IsPlaceholder);
            }

            if (descriptor.CommonElementIdValue.HasValue)
            {
                ElementOptionItem byId = options.FirstOrDefault(
                    x => x.ElementIdValue == descriptor.CommonElementIdValue.Value);

                if (byId != null)
                    return byId;
            }

            ElementOptionItem byName = options.FirstOrDefault(
                x => x.Name == descriptor.DisplayValue);

            return byName ?? options.FirstOrDefault(x => x.IsPlaceholder);
        }

    }
}