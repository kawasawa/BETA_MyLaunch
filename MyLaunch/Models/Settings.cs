using MyBase;
using MyBase.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Media;
using Unity;

namespace MyLaunch.Models
{
    public class Settings : ModelBase
    {
        #region インジェクション

        [Dependency]
        [JsonIgnore]
        public ILoggerFacade Logger { get; set; }

        [Dependency]
        [JsonIgnore]
        public IProductInfo ProductInfo { get; set; }

        #endregion

        #region プロパティ

        private static readonly Encoding FILE_ENCODING = new UTF8Encoding(true);

        [JsonIgnore]
        public IDictionary<string, SolidColorBrush> ColorDefinitions
            => typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static).ToDictionary(p => p.Name, p => new SolidColorBrush((Color)p.GetValue(null)));

        [JsonIgnore]
        public string FilePath => Path.Combine(this.ProductInfo.Roaming, "settings.json");

        private string _version;
        public string Version
        {
            get => this._version;
            set => this.SetProperty(ref this._version, value);
        }

        private string _launchBorderColor = nameof(Colors.Black);
        public string LaunchBorderColor
        {
            get => this._launchBorderColor;
            set
            {
                if (this.SetProperty(ref this._launchBorderColor, value))
                    this.RaisePropertyChanged(nameof(this.LaunchBorderColorBrush));
            }
        }

        [JsonIgnore]
        public SolidColorBrush LaunchBorderColorBrush
        {
            get
            {
                var color = Colors.Black;
                if (string.IsNullOrEmpty(this.LaunchBorderColor) == false)
                    try { color = (Color)ColorConverter.ConvertFromString(this.LaunchBorderColor); } catch { }
                return new SolidColorBrush(color);
            }
        }

        private int _launchBorderSize;
        [Required]
        [Range(1, 10)]
        public int LaunchBorderSize
        {
            get => this._launchBorderSize;
            set => this.SetProperty(ref this._launchBorderSize, value);
        }

        private bool _showLaunchBorderLeft = true;
        public bool ShowLaunchBorderLeft
        {
            get => this._showLaunchBorderLeft;
            set => this.SetProperty(ref this._showLaunchBorderLeft, value);
        }

        private bool _showLaunchBorderTop = true;
        public bool ShowLaunchBorderTop
        {
            get => this._showLaunchBorderTop;
            set => this.SetProperty(ref this._showLaunchBorderTop, value);
        }

        private bool _showLaunchBorderRight = true;
        public bool ShowLaunchBorderRight
        {
            get => this._showLaunchBorderRight;
            set => this.SetProperty(ref this._showLaunchBorderRight, value);
        }

        private bool _showLaunchBorderBottom = true;
        public bool ShowLaunchBorderBottom
        {
            get => this._showLaunchBorderBottom;
            set => this.SetProperty(ref this._showLaunchBorderBottom, value);
        }

        private bool _activateOnRightClick;
        public bool ActivateOnRightClick
        {
            get => this._activateOnRightClick;
            set => this.SetProperty(ref this._activateOnRightClick, value);
        }

        #endregion

        private void InitializeInternal(bool force)
        {
            if (force)
            {
                this.LaunchBorderSize = 1;
            }
            else
            {
                if (this.LaunchBorderSize is < 1 or > 10)
                    this.LaunchBorderSize = 1;
            }
        }

        public bool Initialize(bool force)
        {
            try
            {
                this.InitializeInternal(force);
                this.Logger.Debug($"システム設定を初期化しました。");
                return true;
            }
            catch (Exception e)
            {
                this.Logger.Log($"システム設定の初期化に失敗しました。", Category.Warn, e);
                return false;
            }
        }

        public (bool, Settings) Load()
            => this.Load(this.FilePath);

        public (bool, Settings) Load(string path)
        {
            try
            {
                if (File.Exists(path) == false)
                {
                    this.InitializeInternal(true);
                    return (true, this);
                }

                var json = string.Empty;
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream, FILE_ENCODING))
                {
                    json = reader.ReadToEnd();
                }

                JsonConvert.PopulateObject(json, this);
                this.InitializeInternal(false);

                this.Logger.Debug($"設定ファイルを読み込みました。: Path={path}");
                return (true, this);
            }
            catch (Exception e)
            {
                this.Logger.Log($"設定ファイルの読み込みに失敗しました。: Path={path}", Category.Warn, e);
                return (false, this);
            }
        }

        public bool Save()
            => this.Save(this.FilePath);

        public bool Save(string path)
        {
            try
            {
                this.CleanUp();

                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                using (var writer = new StreamWriter(stream, FILE_ENCODING))
                {
                    writer.Write(json);
                }

                this.Logger.Debug($"設定ファイルを保存しました。: Path={path}");
                return true;
            }
            catch (Exception e)
            {
                this.Logger.Log($"設定ファイルの保存に失敗しました。: Path={path}", Category.Warn, e);
                return false;
            }
        }

        private void CleanUp()
        {
            this.Version = this.ProductInfo.Version.ToString();
        }

        public bool IsDifferentVersion()
            => this.Version != this.ProductInfo.Version.ToString();
    }
}
