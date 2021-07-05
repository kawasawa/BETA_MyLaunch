using GongSolutions.Wpf.DragDrop;
using MyBase;
using MyBase.Wpf.CommonDialogs;
using MyLaunch.Models;
using MyLaunch.Models.LaunchItems;
using MyLaunch.Properties;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Unity;

namespace MyLaunch.ViewModels
{
    public class PreferencesWindowViewModel : ViewModelBase
    {
        private static readonly Encoding FILE_ENCODING = Encoding.UTF8;
        private string AboutPath => Path.Combine(this.ProductInfo.Working, "doc", "ABOUT.md");
        private string DisclaimerPath => Path.Combine(this.ProductInfo.Working, "doc", "DISCLAIMER.md");
        private string HistoryPath => Path.Combine(this.ProductInfo.Working, "doc", "HISTORY.md");
        private string OssLicensePath => Path.Combine(this.ProductInfo.Working, "doc", "OSS_LICENSE.md");
        private string PrivacyPolicyPath => Path.Combine(this.ProductInfo.Working, "doc", "PRIVACY_POLICY.md");

        [Dependency]
        public IProductInfo ProductInfo { get; set; }

        [Dependency]
        public ICommonDialogService CommonDialogService { get; set; }

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

        public IDropTarget DropHandler { get; }

        public ReactiveProperty<string> About { get; }
        public ReactiveProperty<string> Disclaimer { get; }
        public ReactiveProperty<string> History { get; }
        public ReactiveProperty<string> OssLicense { get; }
        public ReactiveProperty<string> PrivacyPolicy { get; }
        public ReactiveProperty<ItemBase> SelectedLaunchItem { get; }

        public ReactiveCommand<bool> AddLinkCommand { get; }
        public ReactiveCommand AddGroupCommand { get; }
        public ReactiveCommand InsertSeparatorCommand { get; }
        public ReactiveCommand RemoveItemCommand { get; }
        public ReactiveCommand<bool> SelectPathCommand { get; }

        public ReactiveCommand<EventArgs> LoadedHandler { get; }
        public ReactiveCommand<EventArgs> ClosedHandler { get; }

        [InjectionConstructor]
        [LogInterceptor]
        public PreferencesWindowViewModel()
        {
            this.DropHandler = new DropHandlerWrapper();

            this.About = new ReactiveProperty<string>()
                .AddTo(this.CompositeDisposable);
            this.Disclaimer = new ReactiveProperty<string>()
                .AddTo(this.CompositeDisposable);
            this.History = new ReactiveProperty<string>()
                .AddTo(this.CompositeDisposable);
            this.OssLicense = new ReactiveProperty<string>()
                .AddTo(this.CompositeDisposable);
            this.PrivacyPolicy = new ReactiveProperty<string>()
                .AddTo(this.CompositeDisposable);

            this.SelectedLaunchItem = new ReactiveProperty<ItemBase>().AddTo(this.CompositeDisposable);

            this.AddLinkCommand = new ReactiveCommand<bool>()
                .WithSubscribe(addDirectory =>
                {
                    FileDialogParametersBase parameter = addDirectory ? new FolderBrowserDialogParameters() : new OpenFileDialogParameters();
                    var ready = this.CommonDialogService.ShowDialog(parameter);
                    if (ready == false)
                        return;

                    var newLink = new Link(parameter.FileName) { IsSelected = true };
                    switch (this.SelectedLaunchItem.Value)
                    {
                        case Group group:
                            group.Children.Add(newLink);
                            break;
                        default:
                            if (this.FindParentDoAction(this.SelectedLaunchItem.Value, (children, index) => children.Insert(index + 1, newLink)) == false)
                                this.LaunchItemSettings.Items.Add(newLink);
                            break;
                    }
                    this.SelectedLaunchItem.Value = newLink;
                })
                .AddTo(this.CompositeDisposable);

            this.AddGroupCommand = new ReactiveCommand()
                .WithSubscribe(() =>
                {
                    var newGroup = new Group(Resources.Label_NewGroup) { IsSelected = true };
                    switch (this.SelectedLaunchItem.Value)
                    {
                        case Group group:
                            group.Children.Add(newGroup);
                            break;
                        default:
                            if (this.FindParentDoAction(this.SelectedLaunchItem.Value, (children, index) => children.Insert(index + 1, newGroup)) == false)
                                this.LaunchItemSettings.Items.Add(newGroup);
                            break;
                    }
                    this.SelectedLaunchItem.Value = newGroup;
                })
                .AddTo(this.CompositeDisposable);

            this.InsertSeparatorCommand = new ReactiveCommand()
                .WithSubscribe(() => this.FindParentDoAction(this.SelectedLaunchItem.Value, (children, index) => children.Insert(index, new Separator())))
                .AddTo(this.CompositeDisposable);

            this.RemoveItemCommand = new ReactiveCommand()
                .WithSubscribe(() => this.FindParentDoAction(this.SelectedLaunchItem.Value, (children, index) => children.RemoveAt(index)))
                .AddTo(this.CompositeDisposable);

            this.SelectPathCommand = new ReactiveCommand<bool>()
                .WithSubscribe(addDirectory =>
                {
                    switch (this.SelectedLaunchItem.Value)
                    {
                        case Link link:
                            FileDialogParametersBase parameter = addDirectory ? new FolderBrowserDialogParameters() : new OpenFileDialogParameters();
                            parameter.InitialDirectory = Path.GetDirectoryName(link.FileName);
                            parameter.DefaultFileName = Path.GetFileName(link.FileName);
                            
                            var ready = this.CommonDialogService.ShowDialog(parameter);
                            if (ready == false)
                                return;

                            link.FileName = parameter.FileName;
                            break;
                        default:
                            break;
                    }
                })
                .AddTo(this.CompositeDisposable);

            this.LoadedHandler = new ReactiveCommand<EventArgs>()
                .WithSubscribe(e =>
                {
                    this.About.Value = File.ReadAllText(this.AboutPath, FILE_ENCODING);
                    this.Disclaimer.Value = File.ReadAllText(this.DisclaimerPath, FILE_ENCODING);
                    this.History.Value = File.ReadAllText(this.HistoryPath, FILE_ENCODING);
                    this.OssLicense.Value = File.ReadAllText(this.OssLicensePath, FILE_ENCODING);
                    this.PrivacyPolicy.Value = File.ReadAllText(this.PrivacyPolicyPath, FILE_ENCODING);
                })
                .AddTo(this.CompositeDisposable);

            this.ClosedHandler = new ReactiveCommand<EventArgs>()
                .WithSubscribe(e =>
                {
                    this.Settings.Save();
                    this.LaunchItemSettings.Save();
                })
                .AddTo(this.CompositeDisposable);
        }

        private bool FindParentDoAction(ItemBase target, Action<ObservableCollection<ItemBase>, int> action)
        {
            var parent = this.LaunchItemSettings;
            var index = parent.Items.IndexOf(target);
            if (0 <= index)
            {
                action(parent.Items, index);
                return true;
            }

            bool findParentDoAction(Group parent)
            {
                var index = parent.Children.IndexOf(target);
                if (0 <= index)
                {
                    action(parent.Children, index);
                    return true;
                }

                foreach (var child in parent.Children.OfType<Group>())
                {
                    if (findParentDoAction(child))
                        return true;
                }
                return false;
            }

            foreach (var child in parent.Items.OfType<Group>())
            {
                if (findParentDoAction(child))
                    return true;
            }
            return false;
        }

        public class DropHandlerWrapper : DefaultDropHandler
        {
            public override void DragOver(IDropInfo dropInfo)
            {
                // 既定のドラッグ処理を実行する
                base.DragOver(dropInfo);

                // ドロップ先がグループの場合は許容する
                if (dropInfo.TargetItem is Group)
                    return;

                // TreeViewItem 間への挿入は許容する
                if (dropInfo.DropTargetAdorner == DropTargetAdorners.Insert)
                    return;

                // いずれにも該当しない場合はドラッグを無効化する
                dropInfo.Effects = System.Windows.DragDropEffects.None;
                dropInfo.DropTargetAdorner = null;
            }

            public override void Drop(IDropInfo dropInfo)
                => base.Drop(dropInfo);
        }
    }
}
