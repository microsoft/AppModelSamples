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

using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml;
using System.Diagnostics;
using Windows.UI.Popups;

namespace TestStartup
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string payload = e.Parameter as string;
            if (!string.IsNullOrEmpty(payload))
            {
                activationText.Text = payload;

                if (payload == "StartupTask")
                {
                    requestButton.IsEnabled = false;
                    requestResult.Text = "Enabled";
                    SolidColorBrush brush = new SolidColorBrush(Colors.Gray);
                    requestResult.Foreground = brush;
                    requestPrompt.Foreground = brush;
                }
            }
        }

        async private void requestButton_Click(object sender, RoutedEventArgs e)
        {
            StartupTask startupTask = await StartupTask.GetAsync("MyStartupId");
            requestResult.Text = startupTask.State.ToString();
            switch (startupTask.State)
            {
                case StartupTaskState.Disabled:
                    // Task is disabled but can be enabled.
                    StartupTaskState newState = await startupTask.RequestEnableAsync();
                    requestResult.Text = newState.ToString();
                    Debug.WriteLine("Request to enable startup, result = {0}", newState);
                    break;
                case StartupTaskState.DisabledByUser:
                    // Task is disabled and user must enable it manually.
                    MessageDialog dialog = new MessageDialog(
                        "I know you don't want this app to run " +
                        "as soon as you sign in, but if you change your mind, " +
                        "you can enable this in the Startup tab in Task Manager.",
                        "TestStartup");
                    await dialog.ShowAsync();
                    break;
                case StartupTaskState.DisabledByPolicy:
                    Debug.WriteLine(
                        "Startup disabled by group policy, or not supported on this device");
                    break;
                case StartupTaskState.Enabled:
                    Debug.WriteLine("Startup is enabled.");
                    break;
            }
        }
    }
}
