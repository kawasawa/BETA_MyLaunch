using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Data;

namespace MyLaunch.Models.LaunchItems
{
    public class Group : ItemBase
    {
        private string _fileName;
        [Required]
        public string FileName
        {
            get => this._fileName;
            set => this.SetProperty(ref this._fileName, value);
        }

        public ObservableCollection<ItemBase> Children { get; set; }

        public Group()
        {
            this.Children = new();
            BindingOperations.EnableCollectionSynchronization(this.Children, new object());
        }

        public Group(string fileName)
            : this()
        {
            this.FileName = fileName;
        }
    }
}
