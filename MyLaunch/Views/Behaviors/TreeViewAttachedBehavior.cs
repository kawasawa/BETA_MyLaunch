using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyLaunch.Views.Behaviors
{
    public class TreeViewAttachedBehavior
    {
        #region SelectOnRightClick

        public static readonly DependencyProperty SelectOnRightClickProperty
            = DependencyPropertyExtensions.RegisterAttached(new PropertyMetadata(OnSelectOnRightClickChanged));
        public static bool GetSelectOnRightClick(DependencyObject obj)
            => (bool)obj.GetValue(SelectOnRightClickProperty);
        public static void SetSelectOnRightClick(DependencyObject obj, bool value)
            => obj.SetValue(SelectOnRightClickProperty, value);

        private static void OnSelectOnRightClickChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not TreeView treeView)
                return;

            if ((bool)e.OldValue)
                treeView.PreviewMouseRightButtonDown -= TreeView_PreviewMouseRightButtonDown;
            if ((bool)e.NewValue)
                treeView.PreviewMouseRightButtonDown += TreeView_PreviewMouseRightButtonDown;
        }

        private static void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (e.OriginalSource as DependencyObject)?.Ancestor().OfType<TreeViewItem>().FirstOrDefault();
            if (item == null)
                return;

            item.Focus();
            e.Handled = true;
        }

        #endregion

        #region ExpandByEnter

        public static readonly DependencyProperty ExpandByEnterProperty
            = DependencyPropertyExtensions.RegisterAttached(new PropertyMetadata(OnExpandByEnterChanged));
        public static bool GetExpandByEnter(DependencyObject obj)
            => (bool)obj.GetValue(ExpandByEnterProperty);
        public static void SetExpandByEnter(DependencyObject obj, bool value)
            => obj.SetValue(ExpandByEnterProperty, value);

        private static void OnExpandByEnterChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not TreeView treeView)
                return;

            if ((bool)e.OldValue)
                treeView.KeyDown -= TreeView_KeyDown;
            if ((bool)e.NewValue)
                treeView.KeyDown += TreeView_KeyDown;
        }

        private static void TreeView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                {
                    if (e.OriginalSource is not TreeViewItem item)
                        return;

                    item.IsExpanded = !item.IsExpanded;
                    e.Handled = true;
                    break;
                }
            }
        }

        #endregion
    }
}
