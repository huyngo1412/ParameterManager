using ParameterManager.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ParameterManager.Views
{
    public partial class ImagePickerWindow : Window
    {
        public IList<ElementOptionItem> Options { get; set; }

        public ElementOptionItem SelectedOption { get; set; }

        public ImagePickerWindow(
            IList<ElementOptionItem> options,
            ElementOptionItem selectedOption)
        {
            InitializeComponent();

            Options = options
                .Where(x => !x.IsPlaceholder)
                .ToList();

            SelectedOption = selectedOption;

            DataContext = this;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}