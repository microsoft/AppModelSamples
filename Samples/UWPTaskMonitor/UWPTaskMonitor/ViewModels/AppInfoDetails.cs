using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;

namespace TaskMonitor.ViewModels
{
    // The AppInfoDetails class is used to display the info for an app on the Details pivot.
    public class AppInfoDetails
    {
        public BitmapImage Logo { get; internal set; }
        public string AppUserModelId { get; internal set; }
        public string Id { get; internal set; }
        public string PackageFamilyName { get; internal set; }
        public string DisplayName { get; internal set; }
        public string Description { get; internal set; }

        public IList<GroupInfoDetails> Groups { get; internal set; }

        public AppInfoDetails(
            BitmapImage logo, string aumid, string id, string pfn, string name, string desc, 
            IList<GroupInfoDetails> g)
        {
            Logo = logo;
            AppUserModelId = aumid;
            Id = id;
            PackageFamilyName = pfn;
            DisplayName = name;
            Description = desc;
            Groups = g;
        }
    }
}
