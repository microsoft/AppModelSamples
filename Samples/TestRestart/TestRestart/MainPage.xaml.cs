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
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace TestRestart
{
    public sealed partial class MainPage : Page
    {
        private bool isAfterLoaded = false;

        public MainPage()
        {
            InitializeComponent();
            App.Images.SelectedImage = "/Assets/london.png";
            DataContext = App.Images;
            imageListView.ItemsSource = App.Images.Items;
            Loaded += MainPage_Loaded;
            LayoutUpdated += MainPage_LayoutUpdated;
        }

        private void MainPage_LayoutUpdated(object sender, object e)
        {
            if (isAfterLoaded)
            {
                restartArgs.Focus(FocusState.Programmatic);
                isAfterLoaded = !isAfterLoaded;
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            isAfterLoaded = true;
        }

        private void imageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (imageListView.SelectedIndex >= 0)
            {
                ImageViewModel item = imageListView.SelectedItem as ImageViewModel;
                if (item != null)
                {
                    App.Images.SelectedImage = item.Image;
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string payload = e.Parameter as string;
            if (!string.IsNullOrEmpty(payload))
            {
                foreach (ImageViewModel imageItem in imageListView.Items)
                {
                    if (imageItem.Name == payload)
                    {
                        imageListView.SelectedItem = imageItem;
                        break;
                    }
                }
            }
        }

        private void restartButton_Click(object sender, RoutedEventArgs e)
        {
            DoRestartRequest();
        }

        private void restartArgs_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                DoRestartRequest();
            }
        }

        async private void DoRestartRequest()
        {
            bool isValidPayload = false;
            string payload = restartArgs.Text;
            if (!string.IsNullOrEmpty(payload))
            {
                foreach (ImageViewModel imageItem in imageListView.Items)
                {
                    if (imageItem.Name == payload)
                    {
                        isValidPayload = true;
                        break;
                    }
                }
            }

            if (isValidPayload)
            {
                AppRestartFailureReason result =
                    await CoreApplication.RequestRestartAsync(payload);
                if (result == AppRestartFailureReason.NotInForeground ||
                    result == AppRestartFailureReason.RestartPending ||
                    result == AppRestartFailureReason.Other)
                {
                    Debug.WriteLine("RequestRestartAsync failed: {0}", result);
                }
            }
        }

    }
}
