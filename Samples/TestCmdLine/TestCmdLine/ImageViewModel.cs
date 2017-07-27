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

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TestCmdLine
{
    public class ImageViewModel
    {
        public string Image { get; set; }
        public string Name { get; set; }
    }

    public class ImagesViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ImageViewModel> Items { get; private set; }

        public ImagesViewModel()
        {
            Items = new ObservableCollection<ImageViewModel>();
            Items.Add(new ImageViewModel()
            {
                Image = "/Assets/Cat.png",
                Name = "Cat"
            });
            Items.Add(new ImageViewModel()
            {
                Image = "/Assets/Dog.png",
                Name = "Dog"
            });
            Items.Add(new ImageViewModel()
            {
                Image = "/Assets/Fish.png",
                Name = "Fish"
            });
            Items.Add(new ImageViewModel()
            {
                Image = "/Assets/Octopus.png",
                Name = "Octopus"
            });
        }

        private string selectedImage;
        public string SelectedImage
        {
            get { return selectedImage; }
            set
            {
                if (selectedImage != value)
                {
                    selectedImage = value;
                    NotifyPropertyChanged("SelectedImage");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
