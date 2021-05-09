using MyBase;
using MyBase.Logging;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using QuickConverter;
using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using WPFLocalizeExtension.Providers;

namespace MyLaunch
{
    /// <summary>
    /// アプリケーションのエントリーポイントとなるクラスを表します。
    /// </summary>
    public partial class App : PrismApplication
    {
        /// <summary>
        /// ロガーを取得します。
        /// </summary>
        public ILoggerFacade Logger { get; }

        /// <summary>
        /// プロダクト情報を取得します。
        /// </summary>
        public IProductInfo ProductInfo { get; }

        /// <summary>
        /// アプリケーションの共有情報を取得します。
        /// </summary>
        public Models.SharedDataService SharedDataService { get; }

        /// <summary>
        /// このクラスの新しいインスタンスを生成します。
        /// </summary>
        public App()
        {
            this.Logger = new CompositeLogger(
            new DebugLogger(),
            new NLogger()
            {
                PublisherType = typeof(ILoggerFacadeExtension),
                ConfigurationFactory = () =>
                {
                    var headerText = new StringBuilder();
                    headerText.AppendLine($"# {this.ProductInfo.Product} ${{var:CTG}} Log");
                    headerText.AppendLine($"# {Environment.OSVersion} - CLR {Environment.Version}");
                    headerText.AppendLine("# ${environment:PROCESSOR_ARCHITECTURE} - ${environment:PROCESSOR_IDENTIFIER}");
                    headerText.AppendLine("# ${environment:COMPUTERNAME}");
                    headerText.Append("##");

                    var header = new NLog.Layouts.CsvLayout();
                    header.Delimiter = NLog.Layouts.CsvColumnDelimiterMode.Tab;
                    header.Quoting = NLog.Layouts.CsvQuotingMode.Nothing;
                    header.Columns.Add(new NLog.Layouts.CsvColumn(string.Empty, headerText.ToString()));

                    var layout = new NLog.Layouts.CsvLayout();
                    layout.Delimiter = NLog.Layouts.CsvColumnDelimiterMode.Tab;
                    layout.Quoting = NLog.Layouts.CsvQuotingMode.Nothing;
                    layout.Header = header;
                    layout.Columns.Add(new NLog.Layouts.CsvColumn(string.Empty, "${longdate}"));
                    layout.Columns.Add(new NLog.Layouts.CsvColumn(string.Empty, "${environment-user}"));
                    layout.Columns.Add(new NLog.Layouts.CsvColumn(string.Empty, $"{this.ProductInfo.Version}"));
                    layout.Columns.Add(new NLog.Layouts.CsvColumn(string.Empty, "${processid}"));
                    layout.Columns.Add(new NLog.Layouts.CsvColumn(string.Empty, "${threadid}"));
                    layout.Columns.Add(new NLog.Layouts.CsvColumn(string.Empty, "${message}"));

                    var file = new NLog.Targets.FileTarget();
                    file.Encoding = Encoding.UTF8;
                    file.Footer = "${newline}";
                    file.FileName = "${var:DIR}/${var:CTG}.log";
                    file.ArchiveFileName = "${var:DIR}/archive/{#}.${var:CTG}.log";
                    file.ArchiveEvery = NLog.Targets.FileArchivePeriod.Day;
                    file.ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Date;
                    file.MaxArchiveFiles = 10;
                    file.Layout = layout;

                    var memory = new NLog.Targets.MemoryTarget();
                    memory.Layout = layout;

                    var config = new NLog.Config.LoggingConfiguration();
                    config.AddTarget(nameof(file), file);
                    config.AddTarget(nameof(memory), memory);
                    config.LoggingRules.Add(new NLog.Config.LoggingRule("*", NLog.LogLevel.Trace, file));
                    config.LoggingRules.Add(new NLog.Config.LoggingRule("*", NLog.LogLevel.Trace, memory));
                    config.Variables.Add("DIR", this.SharedDataService.LogDirectoryPath);
                    return config;
                },
                CreateLoggerHook = (logger, category) =>
                {
                    // ログの種類ごとにファイルを切り替える
                    logger.Factory.Configuration.Variables.Add("CTG", category.ToString());
                    logger.Factory.ReconfigExistingLoggers();
                },
            });
            this.ProductInfo = new ProductInfo();
            this.SharedDataService = new(this.Logger, this.ProductInfo, Process.GetCurrentProcess());
            UnhandledExceptionObserver.Observe(this, this.Logger, this.ProductInfo);
        }

        /// <summary>
        /// アプリケーションの開始直後に行う処理を定義します。
        /// </summary>
        /// <param name="e">イベントの情報</param>
        [LogInterceptor]
        protected override void OnStartup(StartupEventArgs e)
        {
            this.Logger.Log($"アプリケーションを開始しました。", Category.Info);
            this.Logger.Debug($" アプリケーションを開始しました。: Args=[{string.Join(", ", e.Args)}]");

            Initializer.InitQuickConverter();

            this.SharedDataService.CommandLineArgs = e.Args;

            base.OnStartup(e);
        }

        /// <summary>
        /// View から ViewModel を生成するための規則を定義します。
        /// </summary>
        [LogInterceptor]
        protected override void ConfigureViewModelLocator()
        {
            ViewModelLocationProvider.SetDefaultViewModelFactory((view, viewModelType) => {
                // 多言語化の初期設定を行う
                if (view is DependencyObject obj)
                    Initializer.InitWPFLocalizeExtension(obj);

                // ViewModel のインスタンスを生成する
                var viewModel = this.Container.Resolve(viewModelType);
                return viewModel;
            });
        }

        /// <summary>
        /// DI コンテナに登録される型とインスタンスを定義します。
        /// </summary>
        /// <param name="containerRegistry">DI コンテナ</param>
        [LogInterceptor]
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }

        /// <summary>
        /// アプリケーションのメインウィンドウを生成します。
        /// </summary>
        /// <returns>ウィンドウのインスタンス</returns>
        [LogInterceptor]
        protected override Window CreateShell()
        {
            var shell = this.Container.Resolve<Views.MainWindow>();
            shell.Title = this.SharedDataService.Identifier;
            return shell;
        }

        /// <summary>
        /// アプリケーションの終了時に行う処理を定義します。
        /// </summary>
        /// <param name="e">イベントの情報</param>
        [LogInterceptor]
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            this.Logger.Log($"アプリケーションを終了しました。", Category.Info);
            this.Logger.Debug($"アプリケーションを終了しました。: ExitCode={e.ApplicationExitCode}");
        }

        /// <summary>
        /// 初期化処理を行うためのメソッドを提供します。
        /// </summary>
        private static class Initializer
        {
            /// <summary>
            /// <see cref="QuickConverter"/> の初期設定を行います。
            /// </summary>
            [LogInterceptor]
            public static void InitQuickConverter()
            {
#pragma warning disable IDE0049
                EquationTokenizer.AddNamespace(typeof(System.Object));                           // System                  : System.Runtime.dll
                EquationTokenizer.AddNamespace(typeof(System.IO.Path));                          // System.IO               : System.Runtime.dll
                EquationTokenizer.AddNamespace(typeof(System.Text.Encoding));                    // System.Text             : System.Runtime.dll
                EquationTokenizer.AddNamespace(typeof(System.Reflection.Assembly));              // System.Reflection       : System.Runtime.dll
                EquationTokenizer.AddNamespace(typeof(System.Windows.Point));                    // System.Windows          : WindowsBase.dll
                EquationTokenizer.AddNamespace(typeof(System.Windows.UIElement));                // System.Windows          : PresentationCore.dll
                EquationTokenizer.AddNamespace(typeof(System.Windows.Window));                   // System.Windows          : PresentationFramework.dll
                EquationTokenizer.AddNamespace(typeof(System.Windows.Input.Key));                // System.Windows.Input    : WindowsBase.dll
                EquationTokenizer.AddNamespace(typeof(System.Windows.Input.Cursor));             // System.Windows.Input    : PresentationCore.dll
                EquationTokenizer.AddNamespace(typeof(System.Windows.Input.KeyboardNavigation)); // System.Windows.Input    : PresentationFramework.dll
                EquationTokenizer.AddNamespace(typeof(System.Windows.Controls.Control));         // System.Windows.Controls : PresentationFramework.dll
                EquationTokenizer.AddNamespace(typeof(System.Windows.Media.Brush));              // System.Windows.Media    : PresentationFramework.dll
                EquationTokenizer.AddNamespace(typeof(System.Linq.Enumerable));                  // System.Linq             : System.Linq.dll
                EquationTokenizer.AddExtensionMethods(typeof(System.Linq.Enumerable));           // System.Linq             : System.Linq.dll
#pragma warning restore IDE0049
            }

            /// <summary>
            /// 指定された View のインスタンスに対する <see cref="WPFLocalizeExtension"/> の初期設定を行います。
            /// </summary>
            /// <param name="view">View のインスタンス</param>
            [LogInterceptor]
            public static void InitWPFLocalizeExtension(DependencyObject view)
            {
                ResxLocalizationProvider.SetDefaultAssembly(view, nameof(MyLaunch));
                ResxLocalizationProvider.SetDefaultDictionary(view, nameof(MyLaunch.Properties.Resources));
            }
        }
    }
}
