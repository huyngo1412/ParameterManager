using ParameterManager.Common;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ParameterManager.Models
{
    public class FamilyTreeNode : ObservableObject
    {
        private bool? _isChecked = false;
        private bool _isExpanded;

        public string Name { get; set; }

        public string FamilyName { get; set; }

        public long CategoryId { get; set; }

        public long TypeIdValue { get; set; }

        public FamilyTreeNodeKind Kind { get; set; }

        public bool IsCategoryNode => Kind == FamilyTreeNodeKind.Category;

        public bool IsFamilyNode => Kind == FamilyTreeNodeKind.Family;

        public bool IsTypeNode => Kind == FamilyTreeNodeKind.Type;

        public FamilyTreeNode Parent { get; set; }

        public Action SelectionChanged { get; set; }

        public ObservableCollection<FamilyTreeNode> Children { get; set; }
            = new ObservableCollection<FamilyTreeNode>();

        public bool? IsChecked
        {
            get => _isChecked;
            set => SetIsChecked(value, true, true, true);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        private void SetIsChecked(
            bool? value,
            bool updateChildren,
            bool updateParent,
            bool raiseSelectionChanged)
        {
            if (_isChecked == value)
                return;

            _isChecked = value;
            OnPropertyChanged(nameof(IsChecked));

            if (updateChildren && value.HasValue)
            {
                foreach (FamilyTreeNode child in Children)
                {
                    child.SetIsChecked(value, true, false, false);
                }
            }

            if (updateParent)
            {
                Parent?.UpdateCheckStateFromChildren();
            }

            if (raiseSelectionChanged)
            {
                SelectionChanged?.Invoke();
            }
        }

        private void UpdateCheckStateFromChildren()
        {
            if (!Children.Any())
                return;

            bool allChecked = Children.All(x => x.IsChecked == true);
            bool allUnchecked = Children.All(x => x.IsChecked == false);

            bool? newState;

            if (allChecked)
                newState = true;
            else if (allUnchecked)
                newState = false;
            else
                newState = null;

            SetIsChecked(newState, false, false, false);
        }
    }
}
