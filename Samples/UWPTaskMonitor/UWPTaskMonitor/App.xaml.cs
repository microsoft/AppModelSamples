using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TaskMonitor.ViewModels;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using System.Collections.Generic;
using System.Diagnostics;

namespace TaskMonitor
{
    sealed partial class App : Application
    {

        #region Init

        // TODO Needs a valid Mobile Center token.
        private const string MOBILE_CENTER_TOKEN = "<your Mobile Center ID>";

        public App()
        {
            UnhandledException += OnUnhandledException;
            InitializeComponent();
            //Suspending += OnSuspending;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            AnalyticsWriteLine("App.UnhandledException", "Exception", e.ToString());
        }

        public static void AnalyticsWriteLine(string eventName, string header, string payload)
        {
            Debug.WriteLine($"{eventName} | {header} | {payload}");
            Analytics.TrackEvent(eventName, new Dictionary<string, string>{{ header, payload }});
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            MobileCenter.Start(MOBILE_CENTER_TOKEN, typeof(Analytics));
            AnalyticsWriteLine("App.OnLaunched", "LaunchActivatedEventArgs", e.Kind.ToString());

            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                //if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                //{
                //    // Load state from previously suspended application
                //}
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                Window.Current.Activate();
            }
        }

        //private void OnSuspending(object sender, SuspendingEventArgs e)
        //{
        //    SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
        //    // Save application state and stop any background activity
        //    deferral.Complete();
        //}

        #endregion


        #region Display Requests

        // We'll keep the screen alive so long as the app is running.
        private static DisplayRequest appDisplayRequest;
        private static bool isDisplayRequestActive = false;

        internal static void ActivateDisplayRequest()
        {
            if (!isDisplayRequestActive)
            {
                if (appDisplayRequest == null)
                {
                    appDisplayRequest = new DisplayRequest();
                }
                appDisplayRequest.RequestActive();
                isDisplayRequestActive = true;
            }
        }

        #endregion


        #region Version

        private static string versionString = string.Empty;
        public static string VersionString
        {
            get
            {
                if (string.IsNullOrEmpty(versionString))
                {
                    PackageVersion packageVersion = Package.Current.Id.Version;
                    versionString = $"{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}";
                }
                return versionString;
            }
        }

        #endregion


        #region FormFactor

        private static DeviceFormFactorType formFactor = DeviceFormFactorType.Unknown;
        public static DeviceFormFactorType FormFactor
        {
            get
            {
                if (formFactor == DeviceFormFactorType.Unknown)
                {
                    switch (AnalyticsInfo.VersionInfo.DeviceFamily)
                    {
                        case "Windows.Mobile":
                            formFactor = DeviceFormFactorType.Mobile;
                            break;
                        case "Windows.Desktop":
                            formFactor =
                                UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Mouse
                                ? DeviceFormFactorType.Desktop
                                : DeviceFormFactorType.Tablet;
                            break;
                        case "Windows.Universal":
                            formFactor = DeviceFormFactorType.IoT;
                            break;
                        case "Windows.Team":
                            formFactor = DeviceFormFactorType.SurfaceHub;
                            break;
                        case "Windows.Xbox":
                            formFactor = DeviceFormFactorType.Xbox;
                            break;
                        default:
                            formFactor = DeviceFormFactorType.Other;
                            break;
                    }
                    AnalyticsWriteLine("FormFactor", 
                        AnalyticsInfo.VersionInfo.DeviceFamily.ToString(), formFactor.ToString());
                }
                return formFactor;
            }
        }

        #endregion

    }
}