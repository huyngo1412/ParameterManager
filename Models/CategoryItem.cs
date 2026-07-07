using ParameterManager.Common;
using System;

namespace ParameterManager.Models
{
    public class CategoryItem : ObservableObject
    {
        private bool _isChecked;

        public long CategoryId { get; set; }

        public string Name { get; set; }

        public Action SelectionChanged { get; set; }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked == value)
                    return;

                _isChecked = value;
                OnPropertyChanged();

                SelectionChanged?.Invoke();
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
