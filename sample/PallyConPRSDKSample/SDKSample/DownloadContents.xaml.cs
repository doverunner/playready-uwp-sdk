using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using PallyConPRSDKSample.Model;
using System.Diagnostics;

using Windows.Storage;
using System.Threading.Tasks;
using Windows.UI.Popups;

using PallyConSDK;
using PallyConSDK.DownloadTask;

using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Pickers;
using Windows.UI.Core;

namespace PallyConPRSDKSample.SDKSample
{
    public sealed partial class DownloadContents : Page
    {
        private IEnumerable<ContentInfo> _groups;
        private DownloadSendRequest callbackRequest;

        private Dictionary<TimedTextSource, Uri> vttsMap = new Dictionary<TimedTextSource, Uri>();

        public IEnumerable<ContentInfo> Groups
        {
            get { return this._groups; }
        }

        private PallyConPRSDKWrapper PPSDKWrapper;
        //private ProgressBar progress = null;
        private bool stopUpdating = false;

        public DownloadContents()
        {
            this.InitializeComponent();

            PPSDKWrapper = new PallyConPRSDKWrapper();

            // When you want to implement content downloads outside of SDK, you specify a callback function.
            //
            //callbackRequest = new DownloadSendRequest(this.CallbackDownloadTask);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this._groups = ContentInfoDataSource.GetContentsGrouped("Download");
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            //PPSDKWapper.DownloadCancel();
        }

        private void ShowSliptView(object sender, RoutedEventArgs e)
        {
            PallyConPane.PallyConSplitView.IsPaneOpen = !PallyConPane.PallyConSplitView.IsPaneOpen;
        }

        private ContentInfo GetDownloadedContentInfo(string contentName)
        {
            IEnumerator<ContentInfo> e = _groups.GetEnumerator();
            while (e.MoveNext())
            {
                ContentInfo info = e.Current;
                if (info.Title.Equals(contentName))
                {
                    return info;
                }
            }
            return null;
        }

        private async void StartPlayback(ContentInfo info)
        {
            try
            {
                this.DataContext = null;
                info.DownloadPlayUrl = await PPSDKWrapper.GetPlayBackUriAsync(info);
                await PPSDKWrapper.SetPlayReady(mediaElement, info);
                PPSDKWrapper.SetSoftware(mediaElement);
                await PPSDKWrapper.GetLicenseAsync(info);
                this.DataContext = PPSDKWrapper;
                SetPlayerSubtitle(info.DownloadPlayUrl, info);
                mediaElement.Source = info.DownloadPlayUrl;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                PPSDKWrapper.Logger(ex.Message);
            }
        }

        private async void StartDownload(ContentInfo info)
        {
            try
            {
                if (info.IsDownloading == false)
                {
                    info.IsDownloading = true;
                    if (callbackRequest == null)
                        await PPSDKWrapper.DownloadTaskAsync(info, null, DownloadProgress, DownloadComplete, DownloadFail);
                    else
                        await PPSDKWrapper.DownloadTaskAsync(info, callbackRequest, DownloadProgress, DownloadComplete, DownloadFail);

                    await PPSDKWrapper.DownloadStart(info);
                }
                else
                {
                    MessageDialog dialog = new MessageDialog("donwloading...");
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                PPSDKWrapper.DownloadCancel(info);
                ContentDelete(info);
                System.Diagnostics.Debug.WriteLine(ex.Message);
                MessageDialog dialog = new MessageDialog(ex.Message);
                await dialog.ShowAsync();
            }
        }

        private async void DownloadResume(ContentInfo info)
        {
            try
            {
                if (info.IsDownloading == false)
                {
                    info.IsDownloading = true;
                    await PPSDKWrapper.DownloadResume(info);
                }
                else
                {
                    MessageDialog dialog = new MessageDialog("donwloading...");
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                PPSDKWrapper.DownloadCancel(info);
                ContentDelete(info);
                System.Diagnostics.Debug.WriteLine(ex.Message);
                MessageDialog dialog = new MessageDialog(ex.Message);
                await dialog.ShowAsync();
            }
        }

        private async void SetPlayerSubtitle(Uri mpdUri, ContentInfo content)
        {
            try
            {
                MediaSource mediaSource = MediaSource.CreateFromUri(mpdUri);

                var subtitleLists = await PPSDKWrapper.GetSubtitleLIst(mpdUri, content);
                if (subtitleLists.Count != 0)
                {
                    foreach (var subtitle in subtitleLists)
                    {
                        //Debug.WriteLine(subtitle.ToString());
                        var vttUri = new Uri(subtitle);
                        var vtts = TimedTextSource.CreateFromUri(vttUri);
                        vttsMap[vtts] = vttUri;
                        vtts.Resolved += Tts_Resolved;
                        mediaSource.ExternalTimedTextSources.Add(vtts);
                    }

                    MediaPlaybackItem mediaPlaybackItem = new MediaPlaybackItem(mediaSource);
                    mediaElement.SetPlaybackSource(mediaPlaybackItem);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void Tts_Resolved(TimedTextSource sender, TimedTextSourceResolveResultEventArgs args)
        {
            var ttsUri = vttsMap[sender];

            // Handle errors
            if (args.Error != null)
            {
                return;
            }

            // Update label manually since the external SRT does not contain it
            var ttsUriString = ttsUri.AbsoluteUri;
            if (ttsUriString.Contains("_en"))
                args.Tracks[0].Label = "English";
            else if (ttsUriString.Contains("_de"))
                args.Tracks[0].Label = "Deutschland";
            else if (ttsUriString.Contains("_sv"))
                args.Tracks[0].Label = "Swedish";
            else
                args.Tracks[0].Label = "Unkonw";
        }

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            stopUpdating = false;
            Debug.WriteLine(string.Format("You clicked {0}.", e.ClickedItem.ToString()));
            try
            {
                mediaElement.Stop();
                this.DataContext = null;
                ContentInfo info = (ContentInfo)e.ClickedItem;

                this.DataContext = PPSDKWrapper;
                if (info.IsDownloading == true)
                {
                    info.IsDownloading = false;
                    PPSDKWrapper.DownloadPause(info);
                    info.ProgressVisibilty = Visibility.Collapsed;
                }
                else
                {
                    if (info.IsDownloadPaused == true)
                    {
                        this.WhichProgress(true, (ListView)sender, ref info);
                        this.DownloadResume(info);
                    }
                    else
                    {
                        if (await this.IsContentExist(info))
                        {
                            this.StartPlayback(info);
                        }
                        else
                        {
                            this.WhichProgress(true, (ListView)sender, ref info);
                            this.StartDownload(info);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async Task<Boolean> IsContentExist(ContentInfo info)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            if (info.DownloadFolderPath != null)
            {
                storageFolder = await StorageFolder.GetFolderFromPathAsync(info.DownloadFolderPath);
            }
            if (await IfStorageItemExist(storageFolder, info.Title))
            {
                info.ProgressVisibilty = Visibility.Collapsed;
                return true;
            }

            return false;
        }

        public async Task<bool> IfStorageItemExist(StorageFolder folder, string itemName)
        {
            try
            {
                IStorageItem item = await folder.TryGetItemAsync(itemName);
                return (item != null);
            }
            catch (Exception ex)
            {
                // Should never get here 
                PPSDKWrapper.Logger(ex.Message);
                return false;
            }
        }

        private void WhichProgress(Boolean isActivity, ListView listItems, ref ContentInfo info)
        {
            ListView items = listItems;
            int itemCount = 0;
            for (int i = 0; i < items.Items.Count; i++)
            {
                if (info.Url == items.Items[i].ToString())
                {
                    itemCount = i;
                }
            }

            var progress = FindVisualChild<ProgressBar>(items.ContainerFromIndex(itemCount));

            if (isActivity)
            {
                progress.IsIndeterminate = true;
                progress.Visibility = Visibility.Visible;
            }
            else
            {
                progress.IsIndeterminate = false;
                progress.Visibility = Visibility.Collapsed;
            }
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            stopUpdating = true;
            var button = (Button)sender;
            ContentInfo info = button.DataContext as ContentInfo;
            mediaElement.Stop();
            info.IsDownloadPaused = false;

            if (info.IsDownloading)
            {
                info.IsDownloading = false;
                PPSDKWrapper.DownloadCancel(info);
                info.ProgressVisibilty = Visibility.Collapsed;
            }

            ContentDelete(info);
            info.DownloadProgress = 0;
        }

        private async void ContentDelete(ContentInfo info)
        {
            try
            {
                StorageFolder stroageFolder = ApplicationData.Current.LocalFolder;
                if (info.DownloadFolderPath != null)
                {
                    stroageFolder = await StorageFolder.GetFolderFromPathAsync(info.DownloadFolderPath);
                }

                var file = await stroageFolder.GetFolderAsync(info.Title);
                if (file != null)
                {
                    await file.DeleteAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void DownloadProgress(string contentName, int index, int totalIndex)
        {
            if (!stopUpdating)
            {
                Debug.WriteLine("Content Name: {0} TotalIndex: {1} Index: {2}", contentName, totalIndex, index);
                ContentInfo info = GetDownloadedContentInfo(contentName);

                if (info != null)
                {
                    info.DownloadMaxIndex = totalIndex - 1;
                    info.DownloadProgress = index;
                }

                PPSDKWrapper.Logger(contentName + "  Total:" + totalIndex.ToString() + "  Download Index:" + index.ToString());
            }
        }

        private void DownloadComplete(string contentName, StorageFolder fileForder)
        {
            Debug.WriteLine("Content Name: {0} Forder Path: {1}", contentName, fileForder.Path);
            ContentInfo info = GetDownloadedContentInfo(contentName);

            if (info != null)
            {
                info.IsDownloading = false;
                info.ProgressVisibilty = Visibility.Collapsed;
            }

            if (mediaElement.CurrentState != MediaElementState.Playing)
            {
                if (contentName == ((ContentInfo)DownloadListView.SelectedItem).Title)
                    StartPlayback(GetDownloadedContentInfo(contentName));
            }
        }

        private void DownloadFail(string contentName, string failedFileUrl, HttpResponseMessage response)
        {
            Debug.WriteLine("Content Name: {0} Forder Uri: {1}, StatusCode: {2}", contentName, failedFileUrl, response.StatusCode);
            ContentInfo info = GetDownloadedContentInfo(contentName);
            if (info != null)
            {
                info.IsDownloading = false;
                PPSDKWrapper.DownloadCancel(info);
            }
        }

        public Boolean CallbackDownloadTask(string srcUrl, StorageFolder downloadFolder, string destPath)
        {
            var result = Task.Run(() => CallbackTask(srcUrl, downloadFolder, destPath).Result).Result;
            return result;
        }

        public async Task<Boolean> CallbackTask(string srcUrl, StorageFolder downloadFolder, string destPath)
        {
            IHttpFilter filter = new HttpBaseProtocolFilter();
            filter = new PlugInFilter(filter);

            using (HttpClient httpClient = new HttpClient(filter))
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(srcUrl)))
                {
                    using (HttpResponseMessage response = await httpClient.SendRequestAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            // The MPD information calculates the number of files that are segmented, sometimes different from the actual number (one or two).
                            // If you request a file that does not exist, a NotFound will occur, but you must implement it so that it passes properly.
                            // Companies that implement Download Callback functions should develop them in consideration of this.
                            //
                            if (response.StatusCode != HttpStatusCode.NotFound)
                                return false;
                        }

                        using (Stream urlStream = (await response.Content.ReadAsInputStreamAsync()).AsStreamForRead())
                        {
                            var downloadFile = await downloadFolder.CreateFileAsync(
                                        destPath,
                                        CreationCollisionOption.ReplaceExisting);

                            using (Stream fileStream = await downloadFile.OpenStreamForWriteAsync())
                            {
                                await urlStream.CopyToAsync(fileStream, 4096);
                            }

                            return true;
                        }
                    }
                }
            }
        }
    }

    internal class PlugInFilter : IHttpFilter
    {
        private IHttpFilter filter;

        public PlugInFilter(IHttpFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentException("innerFilter cannot be null.");
            }

            this.filter = filter;
        }

        public IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> SendRequestAsync(HttpRequestMessage request)
        {
            return AsyncInfo.Run<HttpResponseMessage, HttpProgress>(async (cancellationToken, progress) =>
            {
                request.Headers.Add("Custom-Header", "CustomRequestValue");
                HttpResponseMessage response = await filter.SendRequestAsync(request).AsTask(cancellationToken, progress);

                cancellationToken.ThrowIfCancellationRequested();

                response.Headers.Add("Custom-Header", "CustomResponseValue");
                return response;
            });
        }

        public void Dispose()
        {
            filter.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
