using MyLaunch.Models;
using MyLaunch.PubSub;
using Prism;
using Prism.Events;
using Prism.Ioc;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyLaunch.Views.Behaviors
{
    public class LaunchBorderAttachedBehavior
    {
        public static readonly DependencyProperty EnabledProperty
            = DependencyPropertyExtensions.RegisterAttached(new PropertyMetadata(OnEnabledChanged));
        public static bool GetEnabled(DependencyObject obj)
            => (bool)obj.GetValue(EnabledProperty);
        public static void SetEnabled(DependencyObject obj, bool value)
            => obj.SetValue(EnabledProperty, value);

        private static void OnEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not Border border)
                return;

            if ((bool)e.OldValue)
            {
                border.ContextMenuOpening -= LaunchBorder_ContextMenuOpening;
                border.MouseLeftButtonDown -= LaunchBorder_MouseLeftButtonDown;
                border.MouseRightButtonDown -= LaunchBorder_MouseRightButtonDown;
            }
            if ((bool)e.NewValue)
            {
                border.ContextMenuOpening += LaunchBorder_ContextMenuOpening;
                border.MouseLeftButtonDown += LaunchBorder_MouseLeftButtonDown;
                border.MouseRightButtonDown += LaunchBorder_MouseRightButtonDown; 
            }
        }

        [LogInterceptor]
        private static void LaunchBorder_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // 右クリックを起点に開かれる本来のコンテキストメニューを無効にする
            e.Handled = true;
        }

        [LogInterceptor]
        private static void LaunchBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var container = ((PrismApplicationBase)Application.Current).Container;
            var eventAggregator = container.Resolve<IEventAggregator>();
            var settings = container.Resolve<Settings>();

            if (settings.ActivateOnRightClick == false)
            {
                if ((sender as FrameworkElement)?.ContextMenu is not ContextMenu contextMenu)
                    return;

                contextMenu.IsOpen = true;
            }
            else
            {
                eventAggregator.GetEvent<ShowPreferencesEvent>().Publish();
            }
        }

        [LogInterceptor]
        private static void LaunchBorder_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var container = ((PrismApplicationBase)Application.Current).Container;
            var eventAggregator = container.Resolve<IEventAggregator>();
            var settings = container.Resolve<Settings>();

            if (settings.ActivateOnRightClick)
            {
                if ((sender as FrameworkElement)?.ContextMenu is not ContextMenu contextMenu)
                    return;

                contextMenu.IsOpen = true;
            }
            else
            {
                eventAggregator.GetEvent<ShowPreferencesEvent>().Publish();
            }
        }
    }
}
