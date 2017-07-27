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
using System.IO;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TestCmdLine
{
    public sealed partial class LoadFilePage : Page
    {
        public LoadFilePage()
        {
            InitializeComponent();
        }

        async protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string payload = e.Parameter as string;
            if (!string.IsNullOrEmpty(payload))
            {
                // We've been activated from the command-line, and asked to load a specific text file.
                try
                {
                    fileNameText.Text = payload;
                    string fileExtension = Path.GetExtension(payload);

                    // Note: we can't set a custom folder location, but at least we can use
                    // a picker to grant the app permissions to the location the user eventually selects.
                    FileOpenPicker picker = new FileOpenPicker();
                    picker.ViewMode = PickerViewMode.List;
                    picker.FileTypeFilter.Clear();
                    picker.FileTypeFilter.Add(fileExtension);

                    StorageFile file = await picker.PickSingleFileAsync();
                    if (file != null)
                    {
                        fileText.Text = await FileIO.ReadTextAsync(file);
                    }
                    else
                    {
                        fileText.Text = "File operation cancelled.";
                    }
                }
                catch (Exception ex)
                {
                    fileText.Text = ex.ToString();
                }
            }
        }

    }
}
