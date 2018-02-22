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
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BananaEdit
{
    public sealed partial class MainPage : Page
    {

        #region Init

        private bool isFileOpen = false;
        private const int CONTROL_VERTICAL_OFFSET = 208;

        public MainPage()
        {
            InitializeComponent();
            App.ActivateDisplayRequest();
            versionText.Text = App.VersionString;
            instanceNumber.Text = Program.InstanceNumber.ToString();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double controlWidth = Math.Max(100, Window.Current.Bounds.Width - 32);
            fileText.Width = controlWidth;
            double fileControlHeight = Math.Max(100, Window.Current.Bounds.Height - CONTROL_VERTICAL_OFFSET);
            fileScrollViewer.Height = fileControlHeight;
            fileScrollViewer.MaxHeight = fileControlHeight;
            double buttonWidth = (controlWidth - 6) / 2;
            closeFile.Width = buttonWidth;
            openFile.Width = buttonWidth;
            fileText.Focus(FocusState.Programmatic);
        }

        #endregion


        #region OnNavigatedTo

        // In the App.OnFileActivated, we always navigate here, and we open the
        // given file unless it's already open.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!isFileOpen)
            {
                // If we're being file-activated, the incoming args will include the file.
                if (e.Parameter is StorageFile file)
                {
                    // Read data from the file, and set it into the UI.
                    OpenFileUpdateUi(file);
                }
                else
                {
                    NoFileUpdateUi("[Not file-activated]");
                }
            }
        }

        private void NoFileUpdateUi(string message)
        {
            Debug.WriteLine(message);
            fileName.Text = string.Empty;
            fileText.Text = message;
            closeFile.IsEnabled = false;
            openFile.IsEnabled = true;
            isFileOpen = false;
        }

        #endregion


        #region Open/Close File

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".banana");

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                OpenFileUpdateUi(file);
            }
            else
            {
                NoFileUpdateUi("[Operation cancelled]");
            }
        }

        private async void OpenFileUpdateUi(StorageFile file)
        {
            // Let's try to register this instance for this file.
            var instance = AppInstance.FindOrRegisterInstanceForKey(file.Name);
            if (instance.IsCurrentInstance)
            {
                Debug.WriteLine($"Registered {App.Id} for {file.Name}");
                // We successfully registered this instance.
                string text = await FileIO.ReadTextAsync(file);
                fileName.Text = file.Name;
                fileText.Text = text;
                closeFile.IsEnabled = true;
                openFile.IsEnabled = false;
                isFileOpen = true;
            }
            else
            {
                // Some other instance registered for this file, so we'll 
                // warn the user and return.
                Debug.WriteLine($"Attempted to open {file.Name} more than once.");
                MessageDialog dialog = new MessageDialog(
                    "You already have this file open in another instance of BananaEdit.");
                await dialog.ShowAsync();
            }
        }

        // When the user is done editing the file, we should change our registration.
        private void CloseFile_Click(object sender, RoutedEventArgs e)
        {
            NoFileUpdateUi("[File closed]");

            // In the simple case, we can just unregister this instance so that
            // it no longer takes part in redirection.
            //AppInstance.Unregister();

            // For more complex behavior, we can re-register ourselves as available for re-use.
            // Multiple instances should be able to register as reusable, and each registration
            // must use a unique key, so we simply append the app's custom Id.
            AppInstance.FindOrRegisterInstanceForKey("REUSABLE" + App.Id.ToString());
        }

        #endregion

    }
}
