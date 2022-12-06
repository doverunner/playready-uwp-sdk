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

using Windows.UI.Popups;
using PallyConPRSDKSample.Model;
using System.Diagnostics;

namespace PallyConPRSDKSample.SDKSample
{
    public sealed partial class Proactive : Page
    {
        private IEnumerable<ContentInfo> _groups;

        public IEnumerable<ContentInfo> Groups
        {
            get { return this._groups; }
        }

        PallyConPRSDKWrapper PPSDKWrapper;

        public Proactive()
        {
            this.InitializeComponent();
            PPSDKWrapper = new PallyConPRSDKWrapper();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Get Content Information from JSON File.
            _groups = ContentInfoDataSource.GetContentsGrouped("Proactive");
        }

        private void ShowSliptView(object sender, RoutedEventArgs e)
        {
            PallyConPane.PallyConSplitView.IsPaneOpen = !PallyConPane.PallyConSplitView.IsPaneOpen;
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine(string.Format("You clicked {0}.", e.ClickedItem.ToString()));

            try
            {
                this.DataContext = null;
                ContentInfo info = (ContentInfo)e.ClickedItem;
                PPSDKWrapper.SetPlayReady(mediaElement, info, true);
                this.DataContext = PPSDKWrapper;
                mediaElement.Source = new Uri(info.Url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async void GetLicense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DataContext = null;
                ContentInfo info = ((Button)sender).DataContext as ContentInfo;
                await PPSDKWrapper.GetLicenseAsync(info);
                PPSDKWrapper.SetPlayReady(mediaElement, info, true);
                this.DataContext = PPSDKWrapper;
                //mediaElement.Source = new Uri(info.Url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.ErrorMessage);
        }
    }
}
