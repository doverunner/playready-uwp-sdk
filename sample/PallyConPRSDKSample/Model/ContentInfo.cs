using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PallyConSDK.DownloadTask;

namespace PallyConPRSDKSample.Model
{
    public class ContentLicenseInfo
    {
        public string Token { get; set; }
        public string CustomData { get; set; }
        public string Url { get; set; }
        public string ExpireDate { get; set; }
        public string HardwearDrm { get; set; }
    }

    public class ContentInfo : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string DrmType { get; set; }
        public ContentLicenseInfo License { get; set; }
        public string Url { get; set; }
        public string Token { get; set; }
        public string CustomData { get; set; }
        public string ContentID { get; set; }
        public string ImagePath { get; set; }
        public string DownloadFolderPath { get; set; }
        public Uri DownloadPlayUrl { get; set; }
        public PallyConDownloadTask DownloadTask { get; set; }
        public Visibility ProgressVisibilty { get; set; }
        public bool IsDownloadPaused { get; set; }

        private bool _isDownloading;
        public bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                if (_isDownloading != value)
                {
                    _isDownloading = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _downloadMaxIndex;
        public double DownloadMaxIndex
        {
            get => _downloadMaxIndex;
            set
            {
                if (_downloadMaxIndex != value)
                {
                    _downloadMaxIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _downloadProgress;
        public double DownloadProgress
        {
            get => _downloadProgress;
            set
            {
                if (_downloadProgress != value)
                {
                    _downloadProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ContentInfo(string title = "", string description = "",
                           string drmtype = "", string url = "",
                           string token = "", string customData = "",
                           string contenId = "", string imagepath = "",
                           string downloadFolderPath = null,
                           Visibility progressVisibilty = Visibility.Visible)
        {
            this.Title = title;
            this.Description = description;
            this.DrmType = drmtype;
            this.Url = url;
            this.Token = token;
            this.CustomData = customData;
            this.ContentID = contenId;
            this.ImagePath = imagepath;
            this.DownloadFolderPath = downloadFolderPath;
            this.ProgressVisibilty = progressVisibilty;
            this.IsDownloadPaused = false;
            this.IsDownloading = false;
            this.DownloadMaxIndex = 0;
            this.DownloadProgress = 0;
        }
        public override string ToString()
        {
            return this.Url;
        }
    }

    public sealed class ContentInfoDataSource
    {
        private static ContentInfoDataSource _contentInfoDataSource = new ContentInfoDataSource();
        private static readonly object _lock = new object();
        private ObservableCollection<ContentInfo> _groups = new ObservableCollection<ContentInfo>();

        public ObservableCollection<ContentInfo> Groups
        {
            get { return this._groups; }
        }

        //public static async ObservableCollection<ContentInfo> GetContentsGrouped(string jsonValue)
        public static ObservableCollection<ContentInfo> GetContentsGrouped(string jsonValue)
        {
            _contentInfoDataSource.GetContentDataAsync(jsonValue);
            return _contentInfoDataSource.Groups;
        }

        private async void GetContentDataAsync(string jsonValue)
        {
            lock (_lock)
            {
                if (this._groups.Count != 0)
                {
                    this._groups.Clear();
                }
            }

            Uri dataUri = new Uri("ms-appx:///Model/ContentData.json");

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string jsonText = await FileIO.ReadTextAsync(file);

            JsonObject jsonObject = JsonObject.Parse(jsonText);
            JsonArray jsonArray = jsonObject[jsonValue].GetArray();

            lock (_lock)
            {
                foreach (IJsonValue groupValue in jsonArray)
                {
                    JsonObject groupObject = groupValue.GetObject();
                    ContentInfo group = new ContentInfo(groupObject["Title"].GetString(),
                                                        groupObject["Description"].GetString(),
                                                        groupObject["DrmType"].GetString(),
                                                        groupObject["Url"].GetString(),
                                                        groupObject["Token"].GetString(),
                                                        groupObject["CustomData"].GetString(),
                                                        groupObject["ContentID"].GetString(),
                                                        groupObject["ImagePath"].GetString());

                    this.Groups.Add(group);
                    DefaultDownloadedContent();
                }
            }
        }

        private void DefaultDownloadedContent()
        {
            StorageFolder stroageFolder = ApplicationData.Current.LocalFolder;
            IEnumerator<ContentInfo> e = this._groups.GetEnumerator();
            while (e.MoveNext())
            {
                ContentInfo info = e.Current;
                try
                {
                    foreach (string directoryName in Directory.GetDirectories(stroageFolder.Path))
                    {
                        string lastFolderName = Path.GetFileName(directoryName);
                        if (info.Title.Equals(lastFolderName))
                        {
                            e.Current.ProgressVisibilty = Visibility.Collapsed;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
