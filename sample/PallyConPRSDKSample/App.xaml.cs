using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PallyConPRSDKSample
{
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the Singleton application object. This is the first line of authoring code that runs, 
        /// so it is logically the same as main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Called when the end user starts the application normally. 
        /// Another entry point is when an application starts, such as opening a specific file.
        /// </summary>
        /// <param name="e">Information about startup requests and processes.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // If the window already has content, do not repeat initializing the app
            // make sure the window is active.
            if (rootFrame == null)
            {
                // Create a frame to use as the navigation context and navigate to the first page.
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO : Loads the status from an application that was previously paused.
                }

                // Insert Frame into Current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // If the navigation stack is not restored, return to the first page
                    // and configure a new page by passing the required information
                    // to the navigation parameters.
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Verify that the current window is an active window
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Called when navigation to a specific page fails
        /// </summary>
        /// <param name="sender">Frames that failed navigation</param>
        /// <param name="e">Information about failed navigation</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Called when an application is suspended from running.
        /// The application state is saved without determining whether the application will terminate
        /// or restart without changing the memory content.
        /// </summary>
        /// <param name="sender">Source of suspend request.</param>
        /// <param name="e">Details of the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // TODO : Save the application state and stop all background operations.
            deferral.Complete();
        }
    }
}
