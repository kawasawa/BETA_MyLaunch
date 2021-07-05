using MyBase;
using MyBase.Logging;
using MyLaunch.Models.LaunchItems;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Unity;

namespace MyLaunch.Models
{
    [JsonConverter(typeof(LaunchSettingsJsonConverter))]
    public class LaunchItemSettings : ModelBase
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
        public string FilePath => Path.Combine(this.ProductInfo.Roaming, "settings.launchitem.json");

        private ObservableCollection<ItemBase> _items = new();
        public ObservableCollection<ItemBase> Items
        {
            get => this._items;
            set => this.SetProperty(ref this._items, value);
        }

        #endregion

        private void InitializeInternal(bool force)
        {
            if (force)
            {
                this.Items = new();
            }
            else
            {
                this.Items ??= new();
            }
        }

        public bool Initialize(bool force)
        {
            try
            {
                this.InitializeInternal(force);
                this.Logger.Debug($"ランチャー設定を初期化しました。");
                return true;
            }
            catch (Exception e)
            {
                this.Logger.Log($"ランチャー設定の初期化に失敗しました。", Category.Warn, e);
                return false;
            }
        }

        public (bool, LaunchItemSettings) Load()
            => this.Load(this.FilePath);

        public (bool, LaunchItemSettings) Load(string path)
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

                // NOTE: JsonConverter を適用させてデシリアライズする
                // JsonConvert.PopulateObject は JsonConverter を使用しないため JsonConvert.DeserializeObject を採用する
                // https://stackoverflow.com/questions/40855380/jsonconvert-populateobject-not-using-jsonconverter-class-attribute
                this.Items = JsonConvert.DeserializeObject<LaunchItemSettings>(json).Items;
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
        }

        private class LaunchSettingsJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
                => objectType == typeof(LaunchItemSettings);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                static IEnumerable<ItemBase> convertToLaunchItems(IEnumerable<JToken> values)
                {
                    foreach (var array in values.OfType<JArray>())
                    {
                        if (array.Path != nameof(LaunchItemSettings.Items) && array.Path.EndsWith(nameof(Group.Children)) == false)
                            continue;
                     
                        foreach (var token in array)
                        {
                            // 項目の型を特定する
                            var kind = token.Children().OfType<JProperty>().FirstOrDefault(c => c.Name == nameof(ItemBase.Kind))?.Value.ToString();
                            var itemType = Type.GetType($"{typeof(ItemBase).Namespace}.{kind},{typeof(ItemBase).Assembly.FullName}");

                            // Group は子要素を再帰的に構築する
                            // Link, Separator はそのまま返却する
                            switch (itemType.Name)
                            {
                                case nameof(Group):
                                    var group = new Group { FileName = token[nameof(Group.FileName)].ToString() };
                                    group.Children.AddRange(convertToLaunchItems(token.Values()));
                                    yield return group;
                                    break;
                                default:
                                    yield return (ItemBase)token.ToObject(itemType);
                                    break;
                            }
                        }
                    }
                }

                var self = new LaunchItemSettings();
                self.Items.AddRange(convertToLaunchItems(JToken.Load(reader).Values()));
                return self;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteStartObject();
                foreach (var property in value.GetType().GetProperties()
                    .Where(p => Attribute.IsDefined(p, typeof(JsonIgnoreAttribute)) == false)
                    .OrderBy(p => (Attribute.GetCustomAttribute(p, typeof(JsonPropertyAttribute)) as JsonPropertyAttribute)?.Order ?? 0))
                {
                    var propertyName = (Attribute.GetCustomAttribute(property, typeof(JsonPropertyAttribute)) as JsonPropertyAttribute)?.PropertyName ?? property.Name;
                    var propertyValue = property.GetValue(value);
                    writer.WritePropertyName(propertyName);
                    serializer.Serialize(writer, propertyValue);
                }
                writer.WriteEndObject();
            }
        }
    }
}