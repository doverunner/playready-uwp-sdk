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
using Windows.Foundation.Collections;
using Windows.Data.Json;
using PallyConPRSDKSample.Model;
using PallyConSDK;

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
                    string test = jsonResponse.ToString();
                    if (jsonResponse.ContainsKey("license"))
                    {
                        string strLicense = jsonResponse["license"].Stringify();
                        // Expiration date and device information are not currently used in the UWP SDK
                        string strExpiredDate = jsonResponse["expire_date"].Stringify();
                        var deviceInfo = jsonResponse["device_info"].GetObject();
                        string deviceId = deviceInfo["device_id"].Stringify();
                        string deviceModel = deviceInfo["device_model"].Stringify();
                        string osVersion = deviceInfo["os_version"].Stringify();

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

        public Proactive()
        {
            this.InitializeComponent();
            PPSDKWrapper = new PallyConPRSDKWrapper();
            PPSDKWrapper.SetPallyConLicenseRequestCallback(LicenseRequestAsync);
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
