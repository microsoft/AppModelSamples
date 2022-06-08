## Introduction
In this example, there are two projects – first is PackageSampleHostedApp which is the host app in C# and second is the PackageSampleChild that is the child app. PackageSampleHostedApp is derived from a publically availble UWP sample app called [Package](https://github.com/microsoft/Windows-universal-samples/tree/main/Samples/Package). Package sample app was migrated to the latest .Net6.0, WindowsAppSDK and WinUI3 and a new Scenario 4 called "Hosted App Management" was added to demo new functionality in Hosted App Model such as "PackageCatalog notifications" and "the new FindRelatedPackages API to support querying Packages with HostRuntimes & their children." This new sample is called PackageSampleHostedApp. 

# App package information sample

Shows how to get package info by using the Windows Runtime packaging API ([Windows.ApplicationModel.Package](http://msdn.microsoft.com/library/windows/apps/br224667) and [Windows.ApplicationModel.PackageId](http://msdn.microsoft.com/library/windows/apps/br224668)).

In addition to the already existing features, the sample demos new functionality in Hosted App Model such as "PackageCatalog notifications" and "the new FindRelatedPackages API to support querying Packages with HostRuntimes & their children." 

## Build the sample

1. If you download the samples ZIP, be sure to unzip the entire archive, not just the folder with the sample you want to build. 
2. Start Microsoft Visual Studio and select **File** \> **Open** \> **Project/Solution**.
3. Starting in the folder where you unzipped the samples, go to the Samples subfolder, then the subfolder for this specific sample and then double-click the Visual Studio Solution (.sln) file.
4. Press Ctrl+Shift+B, or select **Build** \> **Build Solution**.

## Run the Hosted App Managment sample scenario

1. Select Build > Deploy Host app (PackageSampleHostedApp)
2. Select Build > Deploy Child app (PackageSampleChild)
3. Click on "Get Hosted App List" to Child app info such as its name, location, id and to query packages with HostRuntimes & their children
4. Click on "Register for Notifications" to get PackageCatalog notifications
5. Go to start menu and unistall the child app
6. Check back Sample App to see Scenario 4 screen that displays progress notifications 

## About FindRelatedPackages 
This API exposes the dependencies and then dependents for a given package as a Package list. The list can be filtered by the type of dependency using the options parameter such as dependencies/dependents/framework/hostruntime/optional/resource. It will be helpful for any package which provides a Host Runtime, to discover other apps which use it.

## About PackageCatalog notifications
This additional functionality allows Host Runtime Providers to listen specifically to Deployment Notifications of its dependents directly using the options below. 

1. PackageCatalog.OpenForCurrentPackage:
OpenForCurrentPackage() currently allows a package to listen to Deployment notifications about itself and dependent packages. There are 4 types of package dependencies: Optional, Resource, HostRuntime and Framework. We have expanded this functionality to also support HostRuntime dependents. 

2. PackageCatalog.OpenForPackage():
OpenForPackage() will allow listening to any package and its dependents (optional, resource, hostRuntime). This API allows a package (like main Edge package) to listen to Deployment notifications for another package (Edge’s Framework package which acts as a HostRuntime provider) and its dependents(like PWAs).  