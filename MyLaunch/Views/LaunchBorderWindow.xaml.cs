using System.Windows;
using Unity;

namespace MyLaunch.Views
{
    /// <summary>
    /// Interaction logic for LaunchBorderWindow.xaml
    /// </summary>
    public partial class LaunchBorderWindow : Window
    {
        [InjectionConstructor]
        [LogInterceptor]
        public LaunchBorderWindow()
        {
            this.InitializeComponent();
        }
    }
}
