using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TaskMonitor.Controls
{
    public sealed partial class TabHeader : UserControl
    {
        public static readonly DependencyProperty SelectedImageProperty = 
            DependencyProperty.Register("SelectedImage", typeof(string), typeof(TabHeader), null);

        public string SelectedImage
        {
            get { return GetValue(SelectedImageProperty) as string; }
            set { SetValue(SelectedImageProperty, value); }
        }

        public static readonly DependencyProperty UnselectedImageProperty = 
            DependencyProperty.Register("UnselectedImage", typeof(string), typeof(TabHeader), null);

        public string UnselectedImage
        {
            get { return GetValue(UnselectedImageProperty) as string; }
            set { SetValue(UnselectedImageProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty = 
            DependencyProperty.Register("Label", typeof(string), typeof(TabHeader), null);

        public string Label
        {
            get { return GetValue(LabelProperty) as string; }
            set { SetValue(LabelProperty, value); }
        }

        public TabHeader()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void SetSelectedItem(bool isSelected)
        {
            switch(isSelected)
            {
                case true:
                    selectedImage.Visibility = Visibility.Visible;
                    unselectedImage.Visibility = Visibility.Collapsed;
                    break;
                case false:
                    selectedImage.Visibility = Visibility.Collapsed;
                    unselectedImage.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
