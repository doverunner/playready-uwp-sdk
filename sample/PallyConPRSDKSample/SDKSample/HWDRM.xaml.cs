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

namespace PallyConPRSDKSample.SDKSample
{
    public sealed partial class HWDRM : Page
    {
        private IEnumerable<ContentInfo> _groups;

        public IEnumerable<ContentInfo> Groups
        {
            get { return this._groups; }
        }

        private PallyConPRSDKWrapper PPSDKWrapper;

        private Boolean IsHardware = true;

        public HWDRM()
        {
            this.InitializeComponent();
            // Get Content Information from JSON File.
            _groups = ContentInfoDataSource.GetContentsGrouped("HWDRM");
            this.PPSDKWrapper = new PallyConPRSDKWrapper();
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
                mediaElement.Stop();
                ContentInfo info = (ContentInfo)e.ClickedItem;
                if (IsHardware)
                {
                    PPSDKWrapper.SetPlayReady(mediaElement, info);
                }
                else
                {
                    ContentInfo copy = info.Clone();
                    copy.Token = "";
                    PPSDKWrapper.SetPlayReady(mediaElement, copy);
                }
                this.DataContext = PPSDKWrapper;
                mediaElement.Source = new Uri(info.Url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void HardwareDrm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DataContext = null;
                ContentInfo info = ((Button)sender).DataContext as ContentInfo;
                PPSDKWrapper.SetPlayReady(mediaElement, info);
                PPSDKWrapper.SetHardware(mediaElement);
                this.IsHardware = true;
                this.DataContext = PPSDKWrapper;
                mediaElement.Source = new Uri(info.Url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void SoftwareDrm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentInfo info = ((Button)sender).DataContext as ContentInfo;
                ContentInfo copy = info.Clone();
                this.DataContext = null;
                // Since Token information requests a Hardware license, 
                // it creates CustomData to request a license.
                if (copy.Token.Length > 5)
                {
                    copy.Token = " ";
                }
                PPSDKWrapper.SetPlayReady(mediaElement, copy);
                PPSDKWrapper.SetSoftware(mediaElement);
                this.IsHardware = false;
                this.DataContext = PPSDKWrapper;
                mediaElement.Source = new Uri(info.Url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
           
        }
    }
}
