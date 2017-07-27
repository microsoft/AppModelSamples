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

using System.IO;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// NOTE: Aliases are here: %userprofile%\AppData\Local\Microsoft\WindowsApps

namespace TestCmdLine
{
    sealed partial class App : Application
    {

        #region Init

        public static ImagesViewModel Images { get; set; }

        public App()
        {
            InitializeComponent();
            Images = new ImagesViewModel();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }

                rootFrame.Navigated += OnNavigated;
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                SetupBackButton(rootFrame);
                Window.Current.Activate();
            }
        }

        #endregion


        #region BackButton

        private void SetupBackButton(Frame rootFrame)
        {
            // Register a handler for BackRequested events and set the visibility of the software Back button.
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                rootFrame.CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
                e.Handled = true;
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            // Each time a navigation event occurs, update the Back button's visibility.
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ((Frame)sender).CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;
        }

        #endregion


        #region OnActivated

        protected override void OnActivated(IActivatedEventArgs args)
        {
            switch (args.Kind)
            {
                case ActivationKind.CommandLineLaunch:
                    CommandLineActivatedEventArgs cmdLineArgs = 
                        args as CommandLineActivatedEventArgs;
                    CommandLineActivationOperation operation = cmdLineArgs.Operation;
                    string cmdLineString = operation.Arguments;
                    string activationPath = operation.CurrentDirectoryPath;

                    Frame rootFrame = Window.Current.Content as Frame;
                    if (rootFrame == null)
                    {
                        rootFrame = new Frame();
                        Window.Current.Content = rootFrame;
                    }

                    ParsedCommands parsedCommands = 
                        CommandLineParser.ParseUntrustedArgs(cmdLineString);
                    if (parsedCommands != null && parsedCommands.Count > 0)
                    {
                        foreach (ParsedCommand command in parsedCommands)
                        {
                            switch (command.Type)
                            {
                                case ParsedCommandType.SelectItem:
                                    rootFrame.Navigate(typeof(SelectItemPage), command.Payload);
                                    break;
                                case ParsedCommandType.LoadConfig:
                                    rootFrame.Navigate(typeof(LoadConfigPage), command.Payload);
                                    break;
                                case ParsedCommandType.LoadFile:
                                    string filePath = Path.Combine(activationPath, command.Payload);
                                    rootFrame.Navigate(typeof(LoadFilePage), filePath);
                                    break;
                                case ParsedCommandType.Unknown:
                                    rootFrame.Navigate(typeof(HelpPage), cmdLineString);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        rootFrame.Navigate(typeof(MainPage));
                    }

                    Window.Current.Activate();
                    break;
            }
        }

        #endregion

    }
}
