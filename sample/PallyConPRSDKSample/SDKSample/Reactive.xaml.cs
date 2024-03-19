using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Foundation.Collections;
using Windows.Data.Json;
using PallyConSDK;
using PallyConPRSDKSample.Model;


namespace PallyConPRSDKSample.SDKSample
{
    public sealed partial class Reactive : Page
    {
        private IEnumerable<ContentInfo> _groups;
        private Dictionary<TimedTextSource, Uri> vttsMap = new Dictionary<TimedTextSource, Uri>();

        public IEnumerable<ContentInfo> Groups
        {
            get { return this._groups; }
        }

        PallyConPRSDKWrapper PPSDKWrapper;
        private async Task<byte[]> LicenseRequestAsync(Uri requestUrl, byte[] requestBody, IPropertySet requestHeaders)
        {
            HttpContent httpContent = new ByteArrayContent(requestBody);
            foreach (string strHeaderName in requestHeaders.Keys)
            {
                string strHeaderValue = requestHeaders[strHeaderName].ToString();

                // The Add method throws an ArgumentException try to set protected headers like "Content-Type"
                // so set it via "ContentType" property
                if (strHeaderName.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse(strHeaderValue);
                else
                    httpContent.Headers.Add(strHeaderName.ToString(), strHeaderValue);
            }

            System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response = await httpClient.PostAsync(requestUrl, httpContent);
            response.EnsureSuccessStatusCode();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // Check if response_format is in json(custom) format
                if (JsonObject.TryParse(await response.Content.ReadAsStringAsync(), out JsonObject jsonResponse))
                {
                    if (jsonResponse.ContainsKey("license"))
                    {
                        string strLicense = jsonResponse["license"].Stringify();
                        // Expiration date and device information are not currently used in the UWP SDK
                        string strExpiredDate = jsonResponse["expire_date"].Stringify();
                        var deviceInfo = jsonResponse["device_info"].GetObject();
                        string deviceId = deviceInfo["device_id"].Stringify();
                        string deviceModel = deviceInfo["device_model"].Stringify();
                        string osVersion = deviceInfo["os_version"].Stringify();

                        // Returns only the license data parsed from the JSON data
                        return Encoding.UTF8.GetBytes(strLicense);
                    }
                    else if (jsonResponse.ContainsKey("errorCode"))
                    {
                        string errorCode = jsonResponse["errorCode"].Stringify();
                        string errorMessage = jsonResponse["message"].Stringify();
                        throw new LicenseIssuingException("Error code : " + errorCode + "Error Message : " + errorMessage);
                    }
                    else
                    {
                        throw new LicenseIssuingException("Unknown json response : " + jsonResponse.ToString());
                    }
                }
                else
                    return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                var errMsg = "AcquireLicense - Http Response Status Code: " + response.StatusCode.ToString();
                throw new Exception(errMsg);
            }
        }

        public Reactive()
        {
            this.InitializeComponent();
            PPSDKWrapper = new PallyConPRSDKWrapper();
            PPSDKWrapper.SetPallyConLicenseRequestCallback(LicenseRequestAsync);
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
                SetPlayerSubtitle(new Uri(info.Url), info);
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

        private async void SetPlayerSubtitle(Uri mpdUri, ContentInfo content)
        {
            try
            {
                MediaSource mediaSource = MediaSource.CreateFromUri(mpdUri);

                var subtitleList = await PPSDKWrapper.GetSubtitleLIst(mpdUri, content);
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
