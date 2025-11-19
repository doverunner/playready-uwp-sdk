using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DoveRunnerPRSDKSample.Model;
using Windows.Media.Protection;
using Windows.Media.Protection.PlayReady;
using Windows.UI.Xaml.Controls;

using DoveRunnerSDK;
using DoveRunnerSDK.DownloadTask;
using DoveRunnerSDK.DownloadTask.ProxyServer;
using Windows.Storage;
using Windows.Storage.Streams;

namespace DoveRunnerPRSDKSample
{
    class DoveRunnerPRSDKWrapper : DoveRunnerViewModelBase
    {
        // DoveRunner License Acqusition URL
        private const string LA_URL = "https://license-global.pallycon.com/ri/licenseManager.do";
        // DoveRunner PlayReady SDK
        public static PlayReadyDRMSDK PRSDK = PlayReadyDRMSDK.GetInstance;
        // PlayReady Information
        public DoveRunnerPlayReadyInfoViewModel PlayReadyInfo { get; private set; }
        public ContentInfo ContentInfo { get; private set; }
        // Proxy Server for playing Downloaded Content.
        public ProxyServer PROXY = null;

        /// <summary>
        /// Initialize DoveRunner PlayReady SDK
        /// </summary>
        public DoveRunnerPRSDKWrapper()
        {
            try
            {
                PRSDK.Initialize();
                DoveRunnerViewModelBase.ClearLog();
            }
            catch (SDKException e)
            {
                string log = $"{DateTime.Now:HH:mm:ss.fff} - {e.Message}";
                System.Diagnostics.Debug.WriteLine(log);
                DoveRunnerViewModelBase.Log(log);
            }
        }

        /// <summary>
        /// Set up callbacks for DoveRunner license requests.
        /// </summary>
        /// <param name="licenseRequest"> License request callback </param>
        public void SetDoveRunnerLicenseRequestCallback(LicenseRequest licenseRequest)
        {
            PRSDK.SetDoveRunnerLicenseRequestCallback(licenseRequest);
        }

        /// <summary>
        /// Utility function for reading the key table file when applying the licence cipher.
        /// </summary>
        /// <param name="fileName"> Key table filename </param>
        private async Task<IBuffer> ReadFileBufferAsync(string fileName)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/" + fileName));
                IBuffer fileBuffer = await FileIO.ReadBufferAsync(file);
                return fileBuffer;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading file buffer: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Set ProtectionManager on the player before playback.
        /// </summary>
        /// <param name="media">MediaElement</param>
        /// <param name="content">Content Information</param>
        /// <param name="isProactive">Reactive Or Proactive</param>
        public async Task SetPlayReady(MediaElement media, ContentInfo content, string licenseUrl = LA_URL, bool isProactive = false)
        {
            if (media == null)
            {
                throw new ArgumentNullException(nameof(media));
            }

            try
            {
                ContentInfo = new ContentInfo();
                ContentInfo = content;

                //var fileBuffer = await ReadFileBufferAsync("plc-kt-{siteId}.bin");

                // If there is no TOKEN information, the UserID, ContentID and OptionalID can be requested for licensing.
                if (content.Token.Length > 10)
                    ProtectionManager = await PRSDK.CreateProtectionManagerByToken(content.Token, licenseUrl, isProactive/*, fileBuffer*/);
                else if (content.CustomData.Length > 10)
                    ProtectionManager = await PRSDK.CreateProtectionManagerByCustomData(content.CustomData, licenseUrl, isProactive);

                PlayReadyInfo = new DoveRunnerPlayReadyInfoViewModel();
                PlayReadyInfo.RefreshStatics();

                if(ProtectionManager != null)
                    media.ProtectionManager = this.ProtectionManager;
            }
            catch (Exception e)
            {
                string log = $"{DateTime.Now:HH:mm:ss.fff} - {e.Message}";
                System.Diagnostics.Debug.WriteLine(log);
                DoveRunnerViewModelBase.Log(log);
                throw new SDKException(e.Message);
            }
        }

        /// <summary>
        /// Licenses can be downloaded in advance. 
        /// </summary>
        /// <param name="content">contents information</param>
        /// <returns>License Acquisition success or failure</returns>
        public async Task GetLicenseAsync(ContentInfo content)
        {
            PlayReadyInfo = new DoveRunnerPlayReadyInfoViewModel();
            PlayReadyInfo.RefreshStatics();

            try
            {
                if (content.Token.Length > 10)
                    await PRSDK.GetLicense(content.Url, LA_URL, content.Token);
                else if (content.CustomData.Length > 10)
                    await PRSDK.GetLicense(content.Url, LA_URL, content.CustomData);
                else
                    throw new SDKException("Invalid Token or CustomData value.");
            }
            catch (LicenseIssuingException ex)
            {
                DoveRunnerViewModelBase.Log(ex.Message);
            }
            catch (MakeContentHeaderException ex)
            {
                DoveRunnerViewModelBase.Log(ex.Message);
            }
            catch (Exception ex)
            {
                DoveRunnerViewModelBase.Log(ex.Message);
            }
        }


        /// <summary>
        /// When the player plays the content, it plays in Hardware DRM environment.
        /// Licenses for devices and content must support Hardware DRM.
        /// </summary>
        /// <param name="media">MediaElement</param>
        public void SetHardware(MediaElement media)
        {
            PRSDK.ConfigureHardwareDRM(media);
        }

        /// <summary>
        /// When the player plays the content, it plays in Software DRM environment.
        /// </summary>
        /// <param name="media">MediaElement</param>
        public void SetSoftware(MediaElement media)
        {
            PRSDK.ConfigureSoftwareDRM(media);
        }

        /// <summary>
        /// Enter the content information to download and the DownloadTask handler information initially.
        /// It also runs the proxy server needed to play downloaded content.
        /// The downloaded content is stored in the path below.
        /// C:\Users\UserID\AppData\Local\Packages\UWP App ID\LocalState
        /// </summary>
        /// <param name="content">Content Information</param>
        /// <param name="progresshandler">The handler that is called when the download progress changes.</param>
        /// <param name="completehandler">The handler that is called when the download is complete.</param>
        /// <param name="failhandler">The handler that is called when the download fails.</param>
        public async Task DownloadTaskAsync(ContentInfo content, DownloadSendRequest sendrequest, DownloadProgressChangedHandler progresshandler, DownloadCompleteHandler completehandler, DownloadFailHandler failhandler)
        {
            try
            {
                // Proxy Server Create
                //if (PROXY == null)
                {
                    PROXY = await PRSDK.CreateProxyServerAsync(content.DownloadFolderPath);
                }
                // DownloadTask Create
                content.DownloadTask = PRSDK.CreateDownloadTask(content.ContentID, content.Title, content.Url, sendrequest, progresshandler, completehandler, failhandler);
            }
            catch (DownloadFailException ex)
            {
                DoveRunnerViewModelBase.Log(ex.Message);
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
                content.IsDownloadPaused = false;

                if (content.DownloadTask != null)
                {
                    if (content.DownloadFolderPath != null)
                    {
                        await content.DownloadTask.Start(content.DownloadFolderPath);
                    }
                    else
                    {
                        await content.DownloadTask.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                DoveRunnerViewModelBase.Log(ex.Message);
                throw ex;
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
                content.IsDownloadPaused = false;

                if (content.DownloadTask != null)
                    await content.DownloadTask.Resume();
            }
            catch (Exception ex)
            {
                DoveRunnerViewModelBase.Log(ex.Message);
            }
        }

        /// <summary>
        /// Download Cancel.
        /// </summary>
        /// <param name="contentName"></param>
        public void DownloadCancel(ContentInfo content)
        {
            try
            {
                content.IsDownloadPaused = false;

                if (content.DownloadTask != null)
                {
                    content.DownloadTask.Cancel();
                    content.DownloadTask = null;
                }
            }
            catch (Exception ex)
            {
                DoveRunnerViewModelBase.Log(ex.Message);
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
                content.IsDownloadPaused = true;

                if (content.DownloadTask != null)
                    content.DownloadTask.Pause();
            }
            catch (Exception ex)
            {
                DoveRunnerViewModelBase.Log(ex.Message);
            }
        }

        /// <summary>
        /// When the content is downloaded, ask the proxy server to play.(Ex. http://localhost ...)
        /// Set the returned URL to the Player and attempt to play it.
        /// </summary>
        /// <param name="contentName">Enter the downloaded content information.</param>
        /// <returns></returns>
        public async Task<Uri> GetPlayBackUriAsync(ContentInfo content)
        {
            PROXY = await PRSDK.CreateProxyServerAsync(content.DownloadFolderPath);
            return await PROXY.GetContentUri(content.Title);
        }

        public async Task<List<string>> GetSubtitleLIst(Uri localMpdUri, ContentInfo content)
        {
            if (PROXY == null)
            {
                PROXY = await PRSDK.CreateProxyServerAsync(content.DownloadFolderPath);
            }

            return await PROXY.GetSubTitleUrl(localMpdUri, content.Title);
        }

        public void Logger(string message)
        {
            string log = $"{DateTime.Now:HH:mm:ss.fff} - {message}";
            DoveRunnerViewModelBase.Log(log);
        }
    }
}
