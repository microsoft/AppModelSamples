//*********************************************************  
//  
// Copyright (c) Microsoft. All rights reserved.  
// This code is licensed under the MIT License (MIT).  
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF  
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY  
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR  
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.  
//  
//*********************************************************  

using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BananaEdit
{
    sealed partial class App : Application
    {

        #region Init
   
        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
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

        #endregion


        #region VersionString

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


        #region Display Requests

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


        private static Guid id = Guid.NewGuid();
        public static Guid Id { get { return id; } }


        #region OnFileActivated

        // This app cares about file activations, and each activation could 
        // be the result of a redirect.
        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;
            }

            StorageFile file = args.Files.FirstOrDefault() as StorageFile;
            if (rootFrame.Content == null)
            {
                // We always want to navigate to the MainPage, regardless
                // of whether or not this is a redirection.
                rootFrame.Navigate(typeof(MainPage), file);
            }
            Window.Current.Activate();
        }

        #endregion

    }
}
