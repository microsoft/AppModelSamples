// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PackageSampleHostedApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Scenario4 : Page
    {
        // A pointer back to the main page.  This is needed if you want to call methods in MainPage such
        // as NotifyUser()
        MainPage rootPage = MainPage.Current;
        PackageCatalog catalog = null;

        public Scenario4()
        {
            this.InitializeComponent();
        }
        
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
        String versionString(PackageVersion version)
        {
            return String.Format("{0}.{1}.{2}.{3}",
                                 version.Major, version.Minor, version.Build, version.Revision);
        }

        String architectureString(Windows.System.ProcessorArchitecture architecture)
        {
            switch (architecture)
            {
                case Windows.System.ProcessorArchitecture.X86:
                    return "x86";
                case Windows.System.ProcessorArchitecture.Arm:
                    return "arm";
                case Windows.System.ProcessorArchitecture.X64:
                    return "x64";
                case Windows.System.ProcessorArchitecture.Neutral:
                    return "neutral";
                case Windows.System.ProcessorArchitecture.Unknown:
                    return "unknown";
                default:
                    return "???";
            }
        }
        private void GetHostInfo_Click(object sender, RoutedEventArgs e)
        {
            var pkgdependents = PackageRelationship.Dependents;                      
            var hostRuntimeDependents = new FindRelatedPackagesOptions(pkgdependents)
            {
                IncludeFrameworks = false,
                IncludeHostRuntimes = true,
                IncludeOptionals = false,
                IncludeResources = false
            };           
            IList<Windows.ApplicationModel.Package> dependents = Package.Current.FindRelatedPackages(hostRuntimeDependents);
            String output = String.Format("Count: {0}", dependents.Count.ToString());
            for (int i = 0; i < dependents.Count; i++)
            {
                Package dependent = dependents[i];
                output += String.Format("\n[{0}]: {1}", i.ToString(), dependent.Id.FullName);                
                PackageId packageId = dependent.Id;
                output += String.Format("Name: \"{0}\"\n" +
                                              "Version: {1}\n" +
                                              "Architecture: {2}\n" +                                              
                                              "Publisher: \"{3}\"\n" +
                                              "PublisherId: \"{4}\"\n" +
                                              "FullName: \"{5}\"\n" +
                                              "FamilyName: \"{6}\"\n" + "Installed location: \"{7}\"\n\n\n",
                                              packageId.Name,
                                              versionString(packageId.Version),
                                              architectureString(packageId.Architecture),
                                              packageId.Publisher,
                                              packageId.PublisherId,
                                              packageId.FullName,
                                              packageId.FamilyName, 
                                              dependent.InstalledLocation.Path);
            }
            OutputTextBlock.Text = output;                       
        }
        private void PackageInstallingCallback(object x, PackageInstallingEventArgs args)
        {
            Package package = args.Package;
            OutputTextBlock.Text = package.DisplayName;
        }

        private void PackageUninstallingCallback(object x, PackageUninstallingEventArgs args) 

        {            
            if (this.DispatcherQueue.HasThreadAccess)
            {
                OutputTextBlock.Text = OutputTextBlock.Text;
            }
            else
            {
                bool isQueued = this.DispatcherQueue.TryEnqueue(
                Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                () => OutputTextBlock.Text = OutputTextBlock.Text + "\n  PackageUninstalling " + args.Package.Id.FullName + " Progress " + args.Progress + " IsComplete " + args.IsComplete + " ActivityId " + args.ActivityId);

            }
        }
               
        private void OptionalPackageUpdatingCallback(object x, PackageUpdatingEventArgs args)
        {
            Package package = args.TargetPackage;
            OutputTextBlock.Text = package.DisplayName;
        }

        private void Register_for_notifications_Click(object sender, RoutedEventArgs e)
        {
            catalog = PackageCatalog.OpenForPackage(Package.Current);
            OutputTextBlock.Text = "PackageCatalog Opened";
            catalog.PackageUpdating += OptionalPackageUpdatingCallback;
            catalog.PackageInstalling += PackageInstallingCallback;
            catalog.PackageUninstalling += PackageUninstallingCallback;
        }
    }
}
