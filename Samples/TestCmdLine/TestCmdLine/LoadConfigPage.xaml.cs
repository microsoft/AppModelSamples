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
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TestCmdLine
{
    public sealed partial class LoadConfigPage : Page
    {

        #region Initialization

        public LoadConfigPage()
        {
            InitializeComponent();
            CopyFilesFromInstallToLocalFolder();
        }

        string[] configFiles = new string[] { "Config1.txt", "Config2.txt" };
        async private void CopyFilesFromInstallToLocalFolder()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            foreach (string fileName in configFiles)
            {
                bool isFileFound = false;
                try
                {
                    // Try to get the file: if it's not there, this will throw.
                    StorageFile configFile = await localFolder.GetFileAsync(fileName);
                    if (configFile != null)
                    {
                        isFileFound = true;
                    }
                }
                catch (Exception) { }

                // If we didn't find the file in the local folder, copy it there now.
                if (!isFileFound)
                {
                    StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///" + fileName));
                    await file.CopyAsync(localFolder);
                }
            }
        }

        #endregion


        async protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string payload = e.Parameter as string;
            if (!string.IsNullOrEmpty(payload))
            {
                // We've been activated from the command-line, and asked to load a specific config file.
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                try
                {
                    StorageFile configFile = await localFolder.GetFileAsync(payload);
                    configText.Text = await FileIO.ReadTextAsync(configFile);
                    fileNameText.Text = payload;

                    // %userprofile%\AppData\Local\Packages\a50f14e6-8fdd-4793-8253-81f7897a7cb5_4749y9809fq6e\LocalState\Config1.txt
                    Debug.WriteLine(configFile.Path);
                }
                catch (Exception ex)
                {
                    configText.Text = ex.ToString();
                }
            }
        }

    }
}
