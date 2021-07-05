using System.Windows;
using System.Windows.Controls;

namespace MyLaunch.Views.Helpers
{
    public class PreferencesContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LaunchSettingsContentTemplate { get; set; }
        public DataTemplate LaunchItemSettingsContentTemplate { get; set; }
        public DataTemplate AboutContentTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                "LaunchSettings" => this.LaunchSettingsContentTemplate,
                "LaunchItemSettings" => this.LaunchItemSettingsContentTemplate,
                "About" => this.AboutContentTemplate,
                _ => base.SelectTemplate(item, container),
            };
        }
    }
}
