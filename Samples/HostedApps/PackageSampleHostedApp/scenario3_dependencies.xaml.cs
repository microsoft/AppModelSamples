// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel;

namespace PackageSampleHostedApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Scenario3 : Page
    {
        // A pointer back to the main page.  This is needed if you want to call methods in MainPage such
        // as NotifyUser()
        MainPage rootPage = MainPage.Current;

        public Scenario3()
        {
            this.InitializeComponent();
        }

        void GetDependencies_Click(Object sender, RoutedEventArgs e)
        {
            IReadOnlyList<Windows.ApplicationModel.Package> dependencies = Package.Current.Dependencies;

            String output = String.Format("Count: {0}", dependencies.Count.ToString());
            for (int i = 0; i < dependencies.Count; i++)
            {
                Package dependency = dependencies[i];
                output += String.Format("\n[{0}]: {1}", i.ToString(), dependency.Id.FullName);
            }

            OutputTextBlock.Text = output;
        }
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
    }
}
