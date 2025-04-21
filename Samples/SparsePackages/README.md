This repo contains a sample C# application demonstrating how to use [Signed Sparse Packages](https://aka.ms/sparsepkgblog) to give a non-packaged desktop app access to new Windows APIs and features. 

## Instructions

You can learn more about Signed Sparse Packages and Identity, Registration & Activation of  Win32 apps in this [blogpost](https://aka.ms/sparsepkgblog) and in the [documentation](https://aka.ms/sparsepkgdocs).

### Requirements

1. Windows SDK version 10.0.19000.0 +
2. Windows OS version 10.0.19000.0 +
3. Microsoft Visual C++ Redistributables

### PhotoStoreDemo

A non-packaged WPF app that installs a Signed Sparse Package and uses it to act as a Share Target for photos and send + handle Toast notifications. The demo also adds demonstrates how to register with File Explorer as a context menu handler.

* Registration of a Signed Sparse Package and handling of Shared photos + Toast Notifications happens in Startup.cs

* Files to package and sign to create a Sparse Package for use with the app are located in the PhotoStoreDemoPkg directory.
  

### Building and running the sample

1. Make sure your machine has Developer Mode turned on.
2. Retarget the solution to the SDK version on your machine – Right click -> Retarget solution.
3. Add a project reference to the Windows.winmd file at "C:\Program Files (x86)\Windows Kits\10\UnionMetadata\\<SDK_Version>\Windows.winmd". (Right click PhotoStoreDemo project | Add | Reference| Browse | All files | Windows.winmd)
4. Update the Publisher value in the AppxManifest.xml file and in PhotoStoreDemo.exe.manifest to match the Publisher value in your cert. If you need to create a cert for signing have a look at [Creating an app package signing certificate](https://docs.microsoft.com/en-us/windows/win32/appxpkg/how-to-create-a-package-signing-certificate). 
5. Install your cert on the machine
6. Create a Sparse Package by packaging the updated contents of PhotoStoreDemoPkg using [App Packager](https://docs.microsoft.com/en-us/windows/win32/appxpkg/make-appx-package--makeappx-exe-) (MakeAppx.exe) and specifying the **/nv** flag. For example: MakeAppx.exe  pack  /d  \<Path to directory with AppxManifest.xml>  /p  \<Output Path>\\mypackage.msix  /nv  
  - Note: **mypackage.msix** must be part of the **Output Directory**.
7. Sign the new Sparse Package. See [Signing an app package using SignTool](https://docs.microsoft.com/en-us/windows/win32/appxpkg/how-to-sign-a-package-using-signtool) or you can also use [Device Guard Signing](https://docs.microsoft.com/en-us/microsoft-store/device-guard-signing-portal).
8. In the main method (in Startup.cs) update the value of **externalLocation** to match the output location of your VS Build binaries and the value of **sparsePkgPath** to match the path to your signed Sparse Package (.msix). Note that these values cannot be relative paths and must be complete paths.
9. Build the app
10. Copy the PhotoStoreDemoPkg\Assets folder and resources.pri file to the same location as your VS Build binaries
11. Run the app

### Removing the package
If you need to remove the package from package manager you can run the following command in an admin command prompt:

powershell -c “get-appxpackage -name \*photostore\* | remove-appxpackage"
