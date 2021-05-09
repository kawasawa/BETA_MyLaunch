﻿using MyBase;
using MyBase.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MyLaunch.Models
{
    public sealed class SharedDataService
    {
        private readonly ILoggerFacade _logger;
        private readonly IProductInfo _productInfo;

        public Process Process { get; }
        public IEnumerable<string> CommandLineArgs { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> CachedDirectories { get; set; } = Enumerable.Empty<string>();

        public string Identifier => $"__{this._productInfo.Company}:{this._productInfo.Product}:{this._productInfo.Version}__";
        public string LogDirectoryPath => Path.Combine(this._productInfo.Local, "log");
        public string TempDirectoryPath => Path.Combine(this._productInfo.Temporary, this.Process.StartTime.ToString("yyyyMMddHHmmssfff"));

        public SharedDataService(ILoggerFacade logger, IProductInfo productInfo, Process process)
        {
            this._logger = logger;
            this._productInfo = productInfo;
            this.Process = process;
        }

        public void CreateTempDirectory()
        {
            // このプロセスで使用する一時フォルダを作成し、隠し属性を付与する
            var info = new DirectoryInfo(this.TempDirectoryPath);
            info.Create();
            info.Attributes |= FileAttributes.Hidden;

            this._logger.Debug($"一時フォルダを作成し、隠し属性を付与しました。: Path={this.TempDirectoryPath}");
        }
    }
}