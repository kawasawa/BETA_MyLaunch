using Hardcodet.Wpf.TaskbarNotification;
using MyBase;
using MyBase.Logging;
using MyLaunch.Models;
using MyLaunch.PubSub;
using MyLaunch.ViewModels;
using Prism.Events;
using Prism.Ioc;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Unity;

namespace MyLaunch.Views
{
    /// <summary>
    /// Workspace.xaml の相互作用ロジック
    /// </summary>
    public partial class Workspace : Window
    {
        // Constructor Injection
        public IEventAggregator EventAggregator { get; set; }

        // Dependency Injection
        [Dependency]
        public IContainerExtension ContainerExtension { get; set; }
        [Dependency]
        public ILoggerFacade Logger { get; set; }
        [Dependency]
        public IProductInfo ProductInfo { get; set; }
        [Dependency]
        public LaunchItemSettings LaunchItemSettings { get; set; }

        private Window _launchBorder;
        private Window _preferences;

        [InjectionConstructor]
        [LogInterceptor]
        public Workspace(IEventAggregator eventAggregator)
        {
            this.InitializeComponent();
            this.EventAggregator = eventAggregator;

            void showPreferences() => this.ShowPreferences();
            this.EventAggregator.GetEvent<ShowPreferencesEvent>().Subscribe(showPreferences);
        }

        [LogInterceptor]
        private void ShowLaunchBorder()
        {
            if (this._launchBorder != null)
            {
                this._launchBorder.SetForegroundWindow();
                return;
            }

            void launchBorder_Closed(object sender, EventArgs e)
            {
                this._launchBorder.Closed -= launchBorder_Closed;
                this._launchBorder = null;
            }

            this._launchBorder = this.ContainerExtension.Resolve<LaunchBorderWindow>();
            this._launchBorder.Closed += launchBorder_Closed;
            this._launchBorder.Show();
        }

        [LogInterceptor]
        private void ShowPreferences()
        {
            if (this._preferences != null)
            {
                this._preferences.SetForegroundWindow();
                return;
            }

            void preferences_Closed(object sender, EventArgs e)
            {
                this._preferences.Closed -= preferences_Closed;
                this._preferences = null;
            }

            this._preferences = this.ContainerExtension.Resolve<PreferencesWindow>();
            this._preferences.Closed += preferences_Closed;
            this._preferences.Show();
        }


        [LogInterceptor]
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Hide();

            this.ShowLaunchBorder();
            if (this.LaunchItemSettings.Items?.Any() != true)
                this.ShowPreferences();
        }

        [LogInterceptor]
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this._launchBorder?.Dispatcher.InvokeAsync(() => this._launchBorder?.Close());
            this._preferences?.Dispatcher.InvokeAsync(() => this._preferences?.Close());
        }

        [LogInterceptor]
        private void Window_Closed(object sender, EventArgs e)
        {
            this.Descendants().OfType<TaskbarIcon>().ForEach(t => t.Dispose());
        }

        [LogInterceptor]
        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            void viewModel_Disposed(object sender, EventArgs e)
            {
                ((ViewModelBase)sender).Disposed -= viewModel_Disposed;
                this.Dispatcher.InvokeAsync(() => this.Close());
            }

            if (e.OldValue is ViewModelBase oldViewModel)
                oldViewModel.Disposed -= viewModel_Disposed;
            if (e.NewValue is ViewModelBase newViewModel)
                newViewModel.Disposed += viewModel_Disposed;
        }

        [LogInterceptor]
        private void Preferences_Click(object sender, RoutedEventArgs e)
        {
            this.ShowPreferences();
        }

        [LogInterceptor]
        private void TaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.ShowPreferences();
        }
    }
}
