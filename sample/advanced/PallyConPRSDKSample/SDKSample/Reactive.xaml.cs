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
using Windows.System.Display;
using Windows.Media.Core;
using Windows.Media.Playback;

using PallyConSDK;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace PallyConPRSDKSample.SDKSample
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class Reactive : Page
    {
        private IEnumerable<ContentInfo> _groups;
        private Dictionary<TimedTextSource, Uri> vttsMap = new Dictionary<TimedTextSource, Uri>();

        public IEnumerable<ContentInfo> Groups
        {
            get { return this._groups; }
        }

        PallyConPRSDKWrapper PPSDKWrapper;

        public Reactive()
        {
            this.InitializeComponent();
            PPSDKWrapper = new PallyConPRSDKWrapper();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Get Content Information from JSON File.
            _groups = ContentInfoDataSource.GetContentsGrouped("Reactive");
        }

        private void ShowSliptView(object sender, RoutedEventArgs e)
        {
            PallyConPane.PallyConSplitView.IsPaneOpen = !PallyConPane.PallyConSplitView.IsPaneOpen;
        }
        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine(string.Format("You clicked {0}.", e.ClickedItem.ToString()));

            try
            {
                mediaElement.Stop();
                this.DataContext = null;
                ContentInfo info = (ContentInfo)e.ClickedItem;
                await PPSDKWrapper.SetPlayReady(mediaElement, info);
                this.DataContext = PPSDKWrapper;
                SetPlayerSubtitle(new Uri(info.Url), info.Title);
                mediaElement.Source = new Uri(info.Url);
            }
            catch (PallyConSDKException ex)
            {
                PPSDKWrapper.Logger(ex.Message);
                //System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async void SetPlayerSubtitle(Uri mpdUri, string contentName)
        {
            try
            {
                MediaSource mediaSource = MediaSource.CreateFromUri(mpdUri);

                var subtitleList = await PPSDKWrapper.GetSubtitleLIst(mpdUri, contentName);
                if (subtitleList.Count != 0)
                {
                    foreach (var subtitle in subtitleList)
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
        }

    }
}
