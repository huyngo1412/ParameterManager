using ParameterManager.Models;
using ParameterManager.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ParameterManager.Views
{
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();

            DataContextChanged += MainView_DataContextChanged;
        }

        private void MainView_DataContextChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is MainViewModel oldVm)
                oldVm.RequestClose -= Close;

            if (e.NewValue is MainViewModel newVm)
                newVm.RequestClose += Close;
        }

        private void ParameterTreeView_PreviewMouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            DependencyObject source = e.OriginalSource as DependencyObject;

            if (source == null)
                return;

            if (FindVisualParent<TextBox>(source) != null)
                return;

            if (FindVisualParent<ComboBox>(source) != null)
                return;

            TreeViewItem treeViewItem = FindVisualParent<TreeViewItem>(source);

            if (treeViewItem == null)
                return;

            if (treeViewItem.DataContext is not ParameterTreeNode node)
                return;

            if (!node.IsGroup)
                return;

            node.IsExpanded = !node.IsExpanded;
            e.Handled = true;
        }

        private T FindVisualParent<T>(DependencyObject child)
            where T : DependencyObject
        {
            DependencyObject parent = child;

            while (parent != null)
            {
                if (parent is T typedParent)
                    return typedParent;

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }
    }
}