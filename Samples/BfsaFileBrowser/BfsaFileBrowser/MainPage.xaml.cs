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
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace BfsaFileBrowser
{
    public sealed partial class MainPage : Page
    {

        #region Standard stuff

        private const int LIST_HEIGHT_OFFSET = 148;
        private const int TEXT_WIDTH_OFFSET = 156;

        public MainPage()
        {
            InitializeComponent();
            versionText.Text = App.VersionString;
        }

        private void OnPageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double windowWidth = Window.Current.Bounds.Width;
            double windowHeight = Window.Current.Bounds.Height;
            fileTreeView.Width = windowWidth;
            fileTreeView.Height = windowHeight - LIST_HEIGHT_OFFSET;
            pathTextBox.Width = windowWidth - TEXT_WIDTH_OFFSET;
        }

        #endregion


        private void GetFiles_Click(object sender, RoutedEventArgs e)
        {
            GetFiles();
        }

        private void PathTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                GetFiles();
            }
        }

        private async void GetFiles()
        {
            fileTreeView.RootNodes.Clear();
            fileTreeView.Visibility = Visibility.Collapsed;
            string path = pathTextBox.Text;
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    // Get the specified root folder, and then walk down to get all children.
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
                    if (folder != null)
                    {
                        TreeViewNode rootNode = new TreeViewNode() { Content = folder.Name };
                        IReadOnlyList<StorageFolder> folders = await folder.GetFoldersAsync();
                        GetDirectories(folders, rootNode);
                        fileTreeView.RootNodes.Add(rootNode);

                        // UNDONE. For demo purposes, only get folders not files.
                        // Get the files in this folder.
                        //IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                        //foreach (StorageFile file in files)
                        //{
                        //    TreeViewNode fileNode = new TreeViewNode() { Content = file.Name };
                        //    fileTreeView.RootNodes.Add(fileNode);
                        //}
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            fileTreeView.Visibility = Visibility.Visible;
        }

        private async void GetDirectories(IReadOnlyList<StorageFolder> subDirs, TreeViewNode nodeToAddTo)
        {
            try
            {
                foreach (StorageFolder subDir in subDirs)
                {
                    TreeViewNode folderNode = new TreeViewNode() { Content = subDir.Name };
                    IReadOnlyList<StorageFolder> subSubDirs = await subDir.GetFoldersAsync();
                    if (subSubDirs.Count != 0)
                    {
                        // Recurse for sub-folders.
                        GetDirectories(subSubDirs, folderNode);
                    }
                    nodeToAddTo.Children.Add(folderNode);

                    // Get the files in this folder.
                    IReadOnlyList<StorageFile> files = await subDir.GetFilesAsync();
                    foreach (StorageFile file in files)
                    {
                        TreeViewNode fileNode = new TreeViewNode() { Content = file.Name };
                        folderNode.Children.Add(fileNode);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

    }
}
