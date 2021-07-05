using Newtonsoft.Json;

namespace MyLaunch.Models.LaunchItems
{
    public abstract class ItemBase : ModelBase
    {
        [JsonProperty(Order = int.MinValue)]
        public string Kind => this.GetType().Name;

        private bool _isSelected;
        [JsonIgnore]
        public bool IsSelected
        {
            get => this._isSelected;
            set => this.SetProperty(ref this._isSelected, value);
        }
    }
}
