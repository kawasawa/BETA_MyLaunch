using MyLaunch.Models;
using Unity;

namespace MyLaunch.ViewModels
{
    public class LaunchBorderWindowViewModel : ViewModelBase
    {
        private Settings _settings;
        [Dependency]
        public Settings Settings
        {
            get => this._settings;
            set => this.SetProperty(ref this._settings, value);
        }

        private LaunchItemSettings _launchItemSettings;
        [Dependency]
        public LaunchItemSettings LaunchItemSettings
        {
            get => this._launchItemSettings;
            set => this.SetProperty(ref this._launchItemSettings, value);
        }

        [InjectionConstructor]
        [LogInterceptor]
        public LaunchBorderWindowViewModel()
        {
        }
    }
}
