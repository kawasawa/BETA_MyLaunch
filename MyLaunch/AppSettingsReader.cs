using CaseExtensions;
using Microsoft.Extensions.Configuration;
using System;

namespace MyLaunch
{
    /// <summary>
    /// アプリケーション構成ファイルの情報を読み込みます。
    /// </summary>
    public static class AppSettingsReader
    {
        private static readonly Lazy<IConfigurationRoot> _lazyConfiguration
            = new(() =>
            {
                var builder = new ConfigurationBuilder();
                builder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                builder.AddJsonFile("appsettings.json");
                return builder.Build();
            });

        public static string ProjectSite
            => _lazyConfiguration.Value[nameof(ProjectSite).ToSnakeCase()];

        public static string CreatorSite
            => _lazyConfiguration.Value[nameof(CreatorSite).ToSnakeCase()];

        public static string DonationSite
           => _lazyConfiguration.Value[nameof(DonationSite).ToSnakeCase()];
    }
}
