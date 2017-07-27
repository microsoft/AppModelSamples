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

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TestCmdLine
{
    public sealed partial class SelectItemPage : Page
    {
        public SelectItemPage()
        {
            InitializeComponent();
            App.Images.SelectedImage = "/Assets/Cat.png";
            DataContext = App.Images;
            imageListView.ItemsSource = App.Images.Items;
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
                foreach(ImageViewModel imageItem in imageListView.Items)
                {
                    if (imageItem.Name == payload)
                    {
                        imageListView.SelectedItem = imageItem;
                        break;
                    }
                }
            }
        }

    }
}
