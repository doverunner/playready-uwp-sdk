﻿using System;
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

        private Boolean IsHardwareDRM = true;

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

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine(string.Format("You clicked {0}.", e.ClickedItem.ToString()));

            try
            {
                mediaElement.Stop();
                this.DataContext = null;
                ContentInfo info = (ContentInfo)e.ClickedItem;
                await PPSDKWrapper.SetPlayReady(mediaElement, info);

                if (IsHardwareDRM)
                    PPSDKWrapper.SetHardware(mediaElement);
                else
                    PPSDKWrapper.SetSoftware(mediaElement);

                this.DataContext = PPSDKWrapper;
                mediaElement.Source = new Uri(info.Url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                PPSDKWrapper.Logger(ex.Message);
            }
        }

        private async void HardwareDrm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.IsHardwareDRM = true;
                this.DataContext = null;
                ContentInfo info = ((Button)sender).DataContext as ContentInfo;

                if (PPSDKWrapper.ProtectionManager == null)
                    await PPSDKWrapper.SetPlayReady(mediaElement, info);

                PPSDKWrapper.SetHardware(mediaElement);
                this.DataContext = PPSDKWrapper;
                mediaElement.Source = new Uri(info.Url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                PPSDKWrapper.Logger(ex.Message);
            }
        }

        private async void SoftwareDrm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.IsHardwareDRM = false;
                this.DataContext = null;
                ContentInfo info = ((Button)sender).DataContext as ContentInfo;

                if (PPSDKWrapper.ProtectionManager == null)
                    await PPSDKWrapper.SetPlayReady(mediaElement, info);

                PPSDKWrapper.SetSoftware(mediaElement);
                this.DataContext = PPSDKWrapper;
                mediaElement.Source = new Uri(info.Url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                PPSDKWrapper.Logger(ex.Message);
            }

        }
    }
}
