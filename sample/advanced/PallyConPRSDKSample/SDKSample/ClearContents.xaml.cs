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


// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace PallyConPRSDKSample.SDKSample
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class ClearContents : Page
    {
        private IEnumerable<ContentInfo> _groups;
        private PallyConPRSDKWrapper PPWrapper;
        public IEnumerable<ContentInfo> Groups
        {
            get { return this._groups; }
        }

        public ClearContents()
        {
            this.InitializeComponent();
            PPWrapper = new PallyConPRSDKWrapper();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Get Content Information from JSON File.
            _groups = ContentInfoDataSource.GetContentsGrouped("Clear");
        }

        private void ShowSliptView(object sender, RoutedEventArgs e)
        {
            PallyConPane.PallyConSplitView.IsPaneOpen = !PallyConPane.PallyConSplitView.IsPaneOpen;
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine(string.Format("You clicked {0}.", e.ClickedItem.ToString()));
            ContentInfo info = (ContentInfo)e.ClickedItem;
            if (info.Title.Equals("cleardjango") == true)
            {
                StartPlayback(info);
            }
            else
            {
                mediaElement.Source = new Uri(info.Url);
            }
        }

        private async void StartPlayback(ContentInfo info)
        {
            try
            {
                this.DataContext = null;
                info.DownloadPlayUrl = await PPWrapper.GetPlayBackUri(info.Title);
                this.DataContext = PPWrapper;
                mediaElement.Source = info.DownloadPlayUrl;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

    }
}
