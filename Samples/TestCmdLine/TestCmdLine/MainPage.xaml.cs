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

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TestCmdLine
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void selectItemButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SelectItemPage));
        }

        private void loadConfigButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoadConfigPage));
        }

        private void loadFileButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoadFilePage));
        }

        private void helpButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HelpPage));
        }
    }
}
