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

using DoveRunnerPRSDKSample.Model;
using System.Diagnostics;

namespace DoveRunnerPRSDKSample.SDKSample
{
    public sealed partial class ClearContents : Page
    {
        private IEnumerable<ContentInfo> _groups;
        private DoveRunnerPRSDKWrapper PPWrapper;
        public IEnumerable<ContentInfo> Groups
        {
            get { return this._groups; }
        }

        public ClearContents()
        {
            this.InitializeComponent();
            PPWrapper = new DoveRunnerPRSDKWrapper();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Get Content Information from JSON File.
            _groups = ContentInfoDataSource.GetContentsGrouped("Clear");
        }

        private void ShowSliptView(object sender, RoutedEventArgs e)
        {
            DoveRunnerPane.DoveRunnerSplitView.IsPaneOpen = !DoveRunnerPane.DoveRunnerSplitView.IsPaneOpen;
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine(string.Format("You clicked {0}.", e.ClickedItem.ToString()));
            ContentInfo info = (ContentInfo)e.ClickedItem;
            StartPlayback(info);
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            ContentInfo info = new ContentInfo();
            info.Url = UrlInput.Text;
            StartPlayback(info);
        }

        private void StartPlayback(ContentInfo info)
        {
            try
            {
                mediaElement.Stop();
                this.DataContext = null;
                mediaElement.Source = new Uri(info.Url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}
