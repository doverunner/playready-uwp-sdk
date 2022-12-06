using System;
using Windows.ApplicationModel.Core;
using Windows.Media.Protection.PlayReady;
using Windows.UI.Core;


namespace PallyConPRSDKSample.Model
{
    /// <summary>
    /// It shows PlayReady infomation
    /// </summary>
    class PallyConPlayReadyInfoViewModel : PallyConViewModelBase
    {
        public async void RefreshStatics()
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    SecurityLevel = PlayReadyStatics.PlayReadyCertificateSecurityLevel;
                    PlayReadySecurityVersion = PlayReadyStatics.PlayReadySecurityVersion;
                    HasHardwareDRM = PlayReadyStatics.CheckSupportedHardware(PlayReadyHardwareDRMFeatures.HardwareDRM);
                    HasHEVCSupport = PlayReadyStatics.CheckSupportedHardware(PlayReadyHardwareDRMFeatures.HEVC);
                }
                catch
                {
                    PallyConViewModelBase.Log("PlayReadyStatics not yet available");
                }
            });


        }

        private uint playReadySecurityVersion;

        public uint PlayReadySecurityVersion
        {
            get { return playReadySecurityVersion; }
            private set
            {
                if (playReadySecurityVersion != value)
                {
                    playReadySecurityVersion = value;
                    RaisePropertyChanged();
                }
            }
        }


        private uint securityLevel;
        public uint SecurityLevel
        {
            get
            {
                return securityLevel;
            }
            private set
            {
                if (securityLevel != value)
                {
                    securityLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool hasHEVCSupport;
        public bool HasHEVCSupport
        {
            get
            {
                return hasHEVCSupport;
            }
            private set
            {
                if (hasHEVCSupport != value)
                {
                    hasHEVCSupport = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool hasHardwareDRM;
        public bool HasHardwareDRM
        {
            get
            {
                return hasHardwareDRM;
            }
            private set
            {
                if (hasHardwareDRM != value)
                {
                    hasHardwareDRM = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
