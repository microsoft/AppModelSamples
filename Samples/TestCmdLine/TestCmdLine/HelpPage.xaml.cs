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
    public sealed partial class HelpPage : Page
    {
        public HelpPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string payload = e.Parameter as string;
            if (!string.IsNullOrEmpty(payload))
            {
                argumentsText.Text = string.Format("Unknown arguments: {0}", payload);
            }
        }
    }
}
