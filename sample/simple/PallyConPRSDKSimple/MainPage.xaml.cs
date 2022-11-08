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

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x412에 나와 있습니다.

using Windows.Media.Protection;
using Windows.Media.Protection.PlayReady;

// 1. PallyCon PlayReady SDK
using PallyConSDK;

namespace PallyConPRSDKSimple
{

    public sealed partial class MainPage : Page
    {

        // 2. SDK Instance
        public static PallyConPRSDK pallyconsdk = PallyConPRSDK.GetInstance;
        public string SiteID = "Site ID";
        public string SiteKey = "SiteKey";
        public string ContentURL = "Content URL";
        public string Token = "token";
        
        public MainPage()
        {
            this.InitializeComponent();

            try
            {
                // 3. PallyConPR SDK initialize
                pallyconsdk.Initialize(SiteID, SiteKey);

                PallyConPlayReady();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public async void PallyConPlayReady()
        {
            try
            {
                // 4. content license information set
                Player.ProtectionManager = await pallyconsdk.CreateProtectionManagerByToken(Token, false);
                // 5. Play
                Player.Source = new Uri(ContentURL);
                Player.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}
