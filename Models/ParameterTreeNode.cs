using ParameterManager.Common;
using System.Collections.ObjectModel;

namespace ParameterManager.Models
{
    public class ParameterTreeNode : ObservableObject
    {
        private string _value;
        private bool _isEdited;
        private bool _isExpanded = true;
        private bool? _booleanValue;
        private ElementOptionItem _selectedElementOption;

        public string Member { get; set; }

        public string Value
        {
            get => _value;
            set
            {
                if (_value == value)
                    return;

                _value = value;
                OnPropertyChanged();

                if (CanEdit)
                    IsEdited = Value != OriginalValue;
            }
        }

        public string OriginalValue { get; private set; }

        public long? OriginalElementIdValue { get; private set; }

        public long? ElementIdValue { get; set; }

        public bool? OriginalBooleanValue { get; private set; }

        public bool? BooleanValue
        {
            get => _booleanValue;
            set
            {
                if (_booleanValue == value)
                    return;

                _booleanValue = value;
                OnPropertyChanged();

                if (CanEdit && IsBooleanEditor)
                {
                    Value = value.HasValue
                        ? value.Value ? "Yes" : "No"
                        : ParameterConstants.VariesText;

                    IsEdited = BooleanValue != OriginalBooleanValue;
                }
            }
        }

        public bool IsGroup { get; set; }

        public bool CanEdit { get; set; }

        public bool IsReadOnly => !CanEdit;

        public long ParameterId { get; set; }

        public string ParameterName { get; set; }

        public string ParameterSource { get; set; }

        public string StorageType { get; set; }

        public string GroupName { get; set; }

        public ParameterScope Scope { get; set; }

        public ParameterEditorKind EditorKind { get; set; }

        public string ElementOptionKind { get; set; }

        public bool IsBooleanEditor => EditorKind == ParameterEditorKind.BooleanCheckBox;

        public bool IsElementComboEditor => EditorKind == ParameterEditorKind.ElementCombo;

        public bool IsImagePickerEditor => EditorKind == ParameterEditorKind.ImagePicker;

        public string ToolTipText { get; set; }

        public ObservableCollection<ParameterTreeNode> Children { get; set; }
            = new ObservableCollection<ParameterTreeNode>();

        public ObservableCollection<ElementOptionItem> ElementOptions { get; set; }
            = new ObservableCollection<ElementOptionItem>();

        public ElementOptionItem SelectedElementOption
        {
            get => _selectedElementOption;
            set
            {
                if (_selectedElementOption == value)
                    return;

                _selectedElementOption = value;
                OnPropertyChanged();

                if (CanEdit && (IsElementComboEditor || IsImagePickerEditor))
                {
                    ElementIdValue = value?.ElementIdValue;
                    Value = value?.Name ?? string.Empty;
                    IsEdited = ElementIdValue != OriginalElementIdValue;
                }
            }
        }

        public bool IsEdited
        {
            get => _isEdited;
            set
            {
                if (_isEdited == value)
                    return;

                _isEdited = value;
                OnPropertyChanged();
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value)
                    return;

                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        public void SetInitialValue(string value)
        {
            _value = value;
            OriginalValue = value;
            IsEdited = false;

            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(IsEdited));
        }

        public void SetInitialBoolean(bool? value)
        {
            _booleanValue = value;
            OriginalBooleanValue = value;

            _value = value.HasValue
                ? value.Value ? "Yes" : "No"
                : ParameterConstants.VariesText;

            OriginalValue = _value;
            IsEdited = false;

            OnPropertyChanged(nameof(BooleanValue));
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(IsEdited));
        }

        public void SetInitialElementOption(ElementOptionItem option)
        {
            _selectedElementOption = option;
            ElementIdValue = option?.ElementIdValue;
            OriginalElementIdValue = option?.ElementIdValue;

            _value = option?.Name ?? string.Empty;
            OriginalValue = _value;
            IsEdited = false;

            OnPropertyChanged(nameof(SelectedElementOption));
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(IsEdited));
        }
    }
}