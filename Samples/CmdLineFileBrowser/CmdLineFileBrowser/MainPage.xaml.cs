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
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CmdLineFileBrowser
{
    public sealed partial class MainPage : Page
    {

        #region Standard stuff

        private const int LIST_HEIGHT_OFFSET = 148;
        private const int TEXT_WIDTH_OFFSET = 260;

        public MainPage()
        {
            InitializeComponent();
            versionText.Text = App.VersionString;
        }

        private void OnPageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double windowHeight = Window.Current.Bounds.Height;
            double windowWidth = Window.Current.Bounds.Width;
            fileTreeView.Height = Math.Max(100, windowHeight - LIST_HEIGHT_OFFSET);
            fileText.Height = Math.Max(100, windowHeight - LIST_HEIGHT_OFFSET);
            fileText.Width = Math.Max(100, windowWidth - TEXT_WIDTH_OFFSET);
        }

        #endregion


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            string activationPath = e.Parameter as string;
            argumentsText.Text = activationPath;
            fileTreeView.RootNodes.Clear();

            try
            {
                // Get the specified root folder, and then walk down to get all children.
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(activationPath);
                if (folder != null)
                {
                    TreeViewNode rootNode = new TreeViewNode() { Content = folder.Name };
                    IReadOnlyList<StorageFolder> folders = await folder.GetFoldersAsync();
                    GetDirectories(folders, rootNode);
                    fileTreeView.RootNodes.Add(rootNode);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void GetDirectories(IReadOnlyList<StorageFolder> subDirs, TreeViewNode nodeToAddTo)
        {
            try
            {
                foreach (StorageFolder subDir in subDirs)
                {
                    // Recurse this folder to get sub-folder info.
                    TreeViewNode folderNode = new TreeViewNode() { Content = subDir.Name };
                    IReadOnlyList<StorageFolder> subSubDirs = await subDir.GetFoldersAsync();
                    if (subSubDirs.Count != 0)
                    {
                        GetDirectories(subSubDirs, folderNode);
                    }
                    nodeToAddTo.Children.Add(folderNode);

                    // Get the files in this folder.
                    IReadOnlyList<StorageFile> files = await subDir.GetFilesAsync();
                    foreach (StorageFile file in files)
                    {
                        // Store the full path of this file in the node.
                        FileNodeContent content = new FileNodeContent()
                        { FileName = file.Name, FullPath = Path.Combine(subDir.Path, file.Name) };
                        TreeViewNode fileNode = new TreeViewNode() { Content = content };
                        folderNode.Children.Add(fileNode);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void FileTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            try
            {
                string filePath = string.Empty;
                if (args.InvokedItem is TreeViewNode node)
                {
                    // Get the full path of this file from the node.
                    if (node.Content is FileNodeContent content)
                    {
                        filePath = content.FullPath;
                        if (filePath.EndsWith(".txt"))
                        {
                            StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
                            if (file != null)
                            {
                                string text = await FileIO.ReadTextAsync(file);
                                fileText.Text = text;
                            }
                        }
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
