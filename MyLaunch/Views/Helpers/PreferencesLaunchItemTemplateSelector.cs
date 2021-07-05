using System.Windows;
using System.Windows.Controls;

namespace MyLaunch.Views.Helpers
{
    public class PreferencesLaunchItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LinkTemplate { get; set; }
        public DataTemplate GroupTemplate { get; set; }
        public DataTemplate SeparatorTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            switch (item)
            {
                case Models.LaunchItems.Link:
                    return this.LinkTemplate;
                case Models.LaunchItems.Group:
                    return this.GroupTemplate;
                case Models.LaunchItems.Separator:
                    return this.SeparatorTemplate;
                default:
                    return base.SelectTemplate(item, container);
            }
        }
    }
}
