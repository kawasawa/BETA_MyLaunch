using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Unity;

namespace MyLaunch.ViewModels
{
    public class WorkspaceViewModel : ViewModelBase
    {
        public ReactiveCommand ExitApplicationCommand { get; }

        [InjectionConstructor]
        [LogInterceptor]
        public WorkspaceViewModel()
        {
            this.ExitApplicationCommand = new ReactiveCommand()
                .WithSubscribe(() => this.ExitApplication())
                .AddTo(this.CompositeDisposable);
        }

        [LogInterceptor]
        private void ExitApplication()
        {
            this.Dispose();
        }
    }
}
