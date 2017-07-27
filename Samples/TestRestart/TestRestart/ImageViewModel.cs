using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TestRestart
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
                Image = "/Assets/london.png",
                Name = "London"
            });
            Items.Add(new ImageViewModel()
            {
                Image = "/Assets/paris.png",
                Name = "Paris"
            });
            Items.Add(new ImageViewModel()
            {
                Image = "/Assets/seattle.png",
                Name = "Seattle"
            });
            Items.Add(new ImageViewModel()
            {
                Image = "/Assets/vancouver.png",
                Name = "Vancouver"
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
