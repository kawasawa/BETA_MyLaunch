using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Unity;

namespace MyLaunch.Views
{
    /// <summary>
    /// PreferencesWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PreferencesWindow : MahApps.Metro.Controls.MetroWindow
    {
        [InjectionConstructor]
        [LogInterceptor]
        public PreferencesWindow()
        {
            this.InitializeComponent();
        }

        // コンテンツを選択した際にハンバーガーメニューを閉じる
        [LogInterceptor]
        private void Contents_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dependencyObject = Mouse.Captured as DependencyObject;
            while (dependencyObject != null)
            {
                if (dependencyObject is ScrollBar)
                    return;
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }
            this.HamburgerToggleButton.IsChecked = false;
        }
    }
}
