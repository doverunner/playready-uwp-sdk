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

    public class ContentInfo
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string DrmType { get; set; }
        public ContentLicenseInfo License { get; set; }
        public string Url { get; set; }
        public string Token { get; set; }
        public string CustomData { get; set; }
        public string ContentID { get; set; }
        public string UserID { get; set; }
        public string OptionalID { get; set; }
        public string ImagePath { get; set; }

        public Visibility ProgressVisibilty { get; set; }
        public Uri DownloadPlayUrl { get; set; }

        public List<string> Subtitle { get; set; }

        public ContentInfo(string title = "", string description = "", 
                           string drmtype = "", string url = "", 
                           string token = "", string customData = "",
                           string contenId = "", string userid = "", 
                           string optionalid = "", string imagepath = "", 
                           Visibility progressVisibilty = Visibility.Visible)
        {
            this.Title = title;
            this.Description = description;
            this.DrmType = drmtype;
            this.Url = url;
            this.Token = token;
            this.CustomData = customData;
            this.ContentID = contenId;
            this.UserID = userid;
            this.OptionalID = optionalid;
            this.ImagePath = imagepath;
            this.ProgressVisibilty = progressVisibilty;
            this.Subtitle = new List<string>();
        }

        public ContentInfo Clone()
        {
            ContentInfo info = new ContentInfo
            {
                Title = this.Title,
                Url = this.Url,
                Description = this.Description,
                DrmType = this.DrmType,
                Token = this.Token,
                CustomData = this.CustomData,
                ContentID = this.ContentID,
                OptionalID = this.OptionalID,
                ImagePath = this.ImagePath,
                UserID = this.UserID
            };

            return info;
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
                                                        groupObject["UserID"].GetString(),
                                                        groupObject["OptionalID"].GetString(),
                                                        groupObject["ImagePath"].GetString());

                    this.Groups.Add(group);
                    DownloadedContent();
                }
            }
        }

        //private async Task<bool> IfStorageItemExist(StorageFolder folder, string itemName)
        //{
        //    try
        //    {
        //        IStorageItem item = await folder.TryGetItemAsync(itemName);
        //        return (item != null);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Should never get here 
        //        return false;
        //    }
        //}


        private void DownloadedContent()
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
