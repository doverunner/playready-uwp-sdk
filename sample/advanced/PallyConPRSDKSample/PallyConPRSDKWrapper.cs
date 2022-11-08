﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PallyConPRSDKSample.Model;
using Windows.Media.Protection;
using Windows.Media.Protection.PlayReady;
using Windows.UI.Xaml.Controls;

using PallyConSDK;
using PallyConSDK.DownloadTask;
using PallyConSDK.DownloadTask.ProxyServer;
using Windows.Storage;

namespace PallyConPRSDKSample
{
    class PallyConPRSDKWrapper : PallyConViewModelBase
    {
        // PallyCon License Acqusition URL
        private string LA_URL = "https://license.pallycon.com/ri/licenseManager.do";
        // PallyCon Site ID
        private const string SITE_ID = "PallyCon Site ID";
        // PallyCon Site Key
        private const string SITE_KEY = "PallyCon Site Key";
        // PallyCon PlayReady SDK
        public static PallyConPRSDK PPSDK = PallyConPRSDK.GetInstance;
        // PlayReady Information
        public PallyConPlayReadyInfoViewModel PlayReadyInfo { get; private set; }
        public ContentInfo ContentInfo { get; private set; }
        // Contents Download Task
        public PallyConDownloadTask DOWNLOAD_TASK;
        // Proxy Server for playing Downloaded Content.
        public PallyConProxyServer PROXY = null;

        /// <summary>
        /// SITE ID and SITE KEY are the values you get when you join PallyCon. 
        /// </summary>
        /// <param name="siteid"> PallyCon Site ID</param>
        /// <param name="sitekey"> PallyCon Site Key</param>
        public PallyConPRSDKWrapper(string siteid = SITE_ID, string sitekey = SITE_KEY )
        {
            try
            {
                PPSDK.Initialize(siteid, sitekey);
                PallyConViewModelBase.ClearLog();
            }
            catch (PallyConSDKException e)
            {
                string log = $"{DateTime.Now:HH:mm:ss.fff} - {e.Message}";
                System.Diagnostics.Debug.WriteLine(log);
                PallyConViewModelBase.Log(log);
            }
        }

        /// <summary>
        /// Set ProtectionManager on the player before playback.
        /// </summary>
        /// <param name="media">MediaElement</param>
        /// <param name="content">Content Information</param>
        /// <param name="isProactive">Reactive Or Proactive</param>
        public async Task SetPlayReady(MediaElement media, ContentInfo content, bool isProactive = false)
        {
            if (media == null)
            {
                throw new ArgumentNullException(nameof(media));
            }

            try
            {
                ContentInfo = new ContentInfo();
                ContentInfo = content;

                // If there is no TOKEN information, the UserID, ContentID and OptionalID can be requested for licensing.
                if (content.Token.Length > 10)
                    ProtectionManager = await PPSDK.CreateProtectionManagerByToken(content.Token, isProactive);
                else if (content.CustomData.Length > 10)
                    ProtectionManager = await PPSDK.CreateProtectionManagerByCustomData(content.CustomData, isProactive);
                else
                    ProtectionManager = await PPSDK.CreateProtectionManager(content.UserID, content.ContentID, content.OptionalID, isProactive);
                PlayReadyInfo = new PallyConPlayReadyInfoViewModel();
                PlayReadyInfo.RefreshStatics();

                media.ProtectionManager = this.ProtectionManager;
            }
            catch (Exception e)
            {
                string log = $"{DateTime.Now:HH:mm:ss.fff} - {e.Message}";
                System.Diagnostics.Debug.WriteLine(log);
                PallyConViewModelBase.Log(log);
                throw new PallyConSDKException(e.Message);
            }
        }

        /// <summary>
        /// Licenses can be downloaded in advance. 
        /// </summary>
        /// <param name="content">contents information</param>
        /// <returns>License Acquisition success or failure</returns>
        public async Task GetLicenseAsync(ContentInfo content)
        {
            try
            {
                if (content.Token.Length > 10)
                    await PPSDK.GetLicenseByToken(content.Url, LA_URL, content.Token);
                else
                    await PPSDK.GetLicense(content.Url, LA_URL, content.UserID, content.ContentID, content.OptionalID);
            }
            catch (LicenseIssuingException ex)
            {
                PallyConViewModelBase.Log(ex.Message);
            }
            catch (MakeContentHeaderException ex)
            {
                PallyConViewModelBase.Log(ex.Message);
            }
            catch (Exception ex)
            {
                PallyConViewModelBase.Log(ex.Message);
            }
        }


        /// <summary>
        /// When the player plays the content, it plays in Hardware DRM environment.
        /// Licenses for devices and content must support Hardware DRM.
        /// </summary>
        /// <param name="media">MediaElement</param>
        public void SetHardware(MediaElement media)
        {
            PPSDK.ConfigureHardwareDRM(media);
        }

        /// <summary>
        /// When the player plays the content, it plays in Software DRM environment.
        /// </summary>
        /// <param name="media">MediaElement</param>
        public void SetSoftware(MediaElement media)
        {
            PPSDK.ConfigureSoftwareDRM(media);
        }

        /// <summary>
        /// Enter the content information to download and the DownloadTask handler information initially.
        /// It also runs the proxy server needed to play downloaded content.
        /// 다운로드된 컨텐츠는 아래 경로에 저장됩니다.
        /// C:\Users\UserID\AppData\Local\Packages\UWP App ID\LocalState
        /// </summary>
        /// <param name="content">Content Information</param>
        /// <param name="progresshandler">The handler that is called when the download progress changes.</param>
        /// <param name="completehandler">The handler that is called when the download is complete.</param>
        /// <param name="failhandler">The handler that is called when the download fails.</param>
        public void DownloadTaskAsync(ContentInfo content, DownloadSendRequest sendrequest, DownloadProgressChangedHandler progresshandler, DownloadCompleteHandler completehandler, DownloadFailHandler failhandler)
        {
            try
            {
                // Proxy Server Create
                if (PROXY == null)
                {
                    PROXY = PPSDK.CreateProxyServer();
                }
                // DownloadTask Create
                DOWNLOAD_TASK = PPSDK.CreateDownloadTask(content.ContentID, content.Title, content.Url, sendrequest, progresshandler, completehandler, failhandler);
            }
            catch (DownloadFailException ex)
            {
                PallyConViewModelBase.Log(ex.Message);
            }

        }

        /// <summary>
        /// Download Start.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task DownloadStart(ContentInfo content)
        {
            try
            {
                if (DOWNLOAD_TASK != null)
                    await DOWNLOAD_TASK.Start();
            }
            catch (Exception ex)
            {
                PallyConViewModelBase.Log(ex.Message);
            }
        }

        /// <summary>
        /// Download Resume.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task DownloadResume(ContentInfo content)
        {
            try
            {
                if (DOWNLOAD_TASK != null)
                    await DOWNLOAD_TASK.Resume();
            }
            catch (Exception ex)
            {
                PallyConViewModelBase.Log(ex.Message);
            }
        }

        /// <summary>
        /// Download Cancel.
        /// </summary>
        /// <param name="contentName"></param>
        public void DownloadCancel(string contentName)
        {
            try
            {
                if (DOWNLOAD_TASK != null)
                {
                    DOWNLOAD_TASK.Cancel();
                    DOWNLOAD_TASK = null;
                }
            }
            catch (Exception ex)
            {
                PallyConViewModelBase.Log(ex.Message);
            }
        }

        /// <summary>
        /// Download Pause.
        /// </summary>
        /// <param name="content"></param>
        public void DownloadPause(ContentInfo content)
        {
            try
            {
                if (DOWNLOAD_TASK != null)
                    DOWNLOAD_TASK.Pause();
            }
            catch (Exception ex)
            {
                PallyConViewModelBase.Log(ex.Message);
            }
        }

        /// <summary>
        /// When the content is downloaded, ask the proxy server to play.(Ex. http://localhost ...)
        /// Set the returned URL to the Player and attempt to play it.
        /// </summary>
        /// <param name="contentName">Enter the downloaded content information.</param>
        /// <returns></returns>
        public Task<Uri> GetPlayBackUri(string contentName)
        {
            if (PROXY == null)
            {
                PROXY = PPSDK.CreateProxyServer();
            }

            return PROXY.GetContentUri(contentName);
        }

        public Task<List<string>> GetSubtitleLIst(Uri localMpdUri, string contentName)
        {
            if (PROXY == null)
            {
                PROXY = PPSDK.CreateProxyServer();
            }

            return PROXY.GetSubTitleUrl(localMpdUri, contentName);
        }

        public void Logger(string message)
        {
            string log = $"{DateTime.Now:HH:mm:ss.fff} - {message}";
            PallyConViewModelBase.Log(log);
        }
    }
}
