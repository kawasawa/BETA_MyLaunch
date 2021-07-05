using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Vanara.PInvoke;

namespace MyLaunch.Models.LaunchItems
{
    public class Link : ItemBase
    {
        private string _fileName;
        [Required]
        public string FileName
        {
            get => this._fileName;
            set
            {
                if (this.SetProperty(ref this._fileName, value))
                    this.RaisePropertyChanged(nameof(this.Image));
            }
        }

        [JsonIgnore]
        public BitmapSource Image
        {
            get
            {
                var fileName = this.FileName;
                if (string.IsNullOrEmpty(fileName))
                    return null;

                var psfi = new Shell32.SHFILEINFO();
                Shell32.SHGetFileInfo(
                    fileName,
                    Directory.Exists(fileName) ? FileAttributes.Directory : FileAttributes.Normal,
                    ref psfi,
                    Marshal.SizeOf(psfi),
                    Shell32.SHGFI.SHGFI_ICON | Shell32.SHGFI.SHGFI_USEFILEATTRIBUTES | Shell32.SHGFI.SHGFI_SMALLICON);
                if (psfi.hIcon.IsNull)
                    return null;

                return Imaging.CreateBitmapSourceFromHIcon((IntPtr)psfi.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }

        public Link()
        {
        }

        public Link(string fileName)
            : this()
        {
            this.FileName = fileName;
        }
    }
}
