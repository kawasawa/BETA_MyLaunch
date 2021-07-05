using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MyLaunch.Views.Behaviors
{
    public class LaunchMenuItemAttachedBehavior
    {
        public static readonly DependencyProperty EnabledProperty
            = DependencyPropertyExtensions.RegisterAttached(new PropertyMetadata(OnEnabledChanged));
        public static bool GetEnabled(DependencyObject obj)
            => (bool)obj.GetValue(EnabledProperty);
        public static void SetEnabled(DependencyObject obj, bool value)
            => obj.SetValue(EnabledProperty, value);

        private static void OnEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;

            if ((bool)e.OldValue)
            {
                menuItem.Click -= LaunchMenuItem_Click;
            }
            if ((bool)e.NewValue)
            {
                menuItem.Click += LaunchMenuItem_Click; 
            }
        }

        [LogInterceptor]
        private static void LaunchMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not Models.LaunchItems.Link link)
                return;

            if (Directory.Exists(link.FileName))
                Process.Start("explorer.exe", link.FileName);
            else
                Process.Start(new ProcessStartInfo("cmd", $"/c start \"\" \"{link.FileName}\"") { CreateNoWindow = true });
        }
    }
}
