This repo contains 2 sample applications demonstrating how to use the Hosted AppModel to define a host and a hosted app.

## Instructions
You can learn more about the Hosted AppModel in this documentation.

### Requirements:
1. Windows SDK version 10.0.19041.0 or higher
2. Windows OS version 10.0.19041.0 or higher

## WinformsToastHost.Package
This package contains a simple Windows Forms application that displays the package's identity & 
location and can show a simple toast notification to launch a new instance of the app. 

It also has the capability to be a host for other apps, in which case it loads a well-known DLL
from the hosted app's install directory and calls into the DLL to perform the app-specific 
behavior.

The app is packaged with the Windows Application Packaging Project which includes the 
manifest declarations for being a host:

```
<Extensions>
  <uap10:Extension Category="windows.hostRuntime" Executable="WinformsToastHost\WinformsToastHost.exe"
        uap10:RuntimeBehavior="packagedClassicApp"
        uap10:TrustLevel="mediumIL">
    <uap10:HostRuntime Id="WinformsToastHost"/>
  </uap10:Extension>
</Extensions>
```

### HostedAppExtension.Package
This package includes a simple DLL that implements the app-specific logic for an app
that relies on the `WinformsToastHost` app to do the heavy lifting. The extension mechanism
is trivial and all the extension does is show a `MessageBox`.

The app is packaged with the Windows Application Packaging Project which includes the 
manifest declarations for relying on a host app package and the entry-point within the
host's package (in this case, `WinformsHostToast`):

```
<Dependencies>
  <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.19569.0" MaxVersionTested="10.0.19569.0" />
  <uap10:HostRuntimeDependency Name="WinformsToastHost" Publisher="CN=AppModelSamples" MinVersion="1.0.0.0" />
</Dependencies>

<!-- ... -->

  <Application Id="App" uap10:HostId="WinformsToastHost">
    <!-- ... -->
  </Application>
```

### Building and running the sample
1. Make sure your machine has Developer Mode turned on.
1. Open WinformsToastHost.sln in VS2019.
1. Retarget the `WinformsToastHost.Package` project to the 10.0.19041 or higher SDK version installed on your machine.
1. Build and deploy `WinformsToastHost.Package`.
1. Build and deploy `HostedAppExtension.Package`.
1. Goto Start menu and launch `WinformsToastHost`.
   * Note that there is only one button in the app.
   * Clicking the "Show Toast" button will show a toast -- if you click on the toast, a new
instance of the app will start with the same identity.
1. Goto Start menu and launch `Hosted WinformsToastHost Extension`.
   * Note that there are now two buttons on the app, and its identity is different than before.
   * Clicking the "Show Toast" button will show a toast -- if you click on the toast, a new
instance of the app will start with the same hosted app's identity.
   * Clicking the "Run hosted app" button will load and run the extension provided by
the hosted app.

In a real app, the host would automatically load and execute the hosted app's code; this sample requires
you to press the button to do so.

## Python host plus hosted apps
In this example, the host is comprised of 2 projects – first is PyScriptEngine which is wrapper written in C# and makes use of the Python nuget package to run python scripts. 
This wrapper parses the command line and has the capability to register a manifest as well as launch the python executable with a path to a script file. 
The second project is PyScriptEnginePackage which is a Windows Application Packaging Project that installs PyScriptEngine and registers the manifest that includes the HostRuntime extension.
### NumberGuesser
The Hosted App is made up of a python script and visual assets. It doesn’t contain any executable files. 
It includes an application manifest where the declarations for `HostRuntimeDependency` and `HostId` are declared that identifies PyScriptEngine as its Host. 
The manifest also contains the Unsigned Publisher OID entry that is required for an unsigned package.
### Show Headers
This Hosted App is a simple script that will dump the HTTP headers from any URI that you Share with it via Windows' "Share" feature. Unlike the
Number Guessing game, this sample doesn't include an MSIX project but relies only on the Python script, the AppXManifest, and a logo image. This
is about the most basic a Hosted App can be.

### Building and running the sample
1. Make sure your machine has Developer Mode turned on.
1. Open PyScriptEngine.sln solution in Visual Studio
1. Retarget the `PyScriptEnginePackage` project to the 10.0.19041 SDK or higher version installed on your machine.
1. Build & Deploy `PyScriptEnginePackage` 
1. Because the host app declares an appexecutionalias, you now go to a command prompt and run "pyengine" to get the usage notice:

    > C:\repos\AppModelSamples>pyscriptengine<br>
    > PyScriptEngine.exe, a simple host for running Python scripts.<br>
    > See https://github.com/microsoft/AppModelSamples for source.
    > 
    > Usage:
    > 
    >   To register a loose package:
    > 
    >     PyScriptEngine.exe -Register <AppXManifest.xml>
    > 
    >   To register an MSIX package:
    > 
    >     PyScriptEngine.exe -AddPackage <MSIX-file> [-unsigned]
    > 
    >     The optional -unsigned parameter is used if the package is unsigned.
    >     In this case, the package cannot include any executable files; only
    >     content files (like .py scripts or images) for the Host to execute.
    > 
    >   To run a registered package, run it from the Start Menu.

### Register the Number Guessing game

1. There are two ways to register the NumberGuesser Hosted App, (a) with the loose file `AppxManifest.xml` or (b) building the msix package.<br>
    (a) To register the hosted application via the file manifest run the following from the commandline: 

    > C:\repos\AppModelSamples\Samples\HostedApps\Python-NumberGuesser>pyscriptengine -register .\NumberGuesser\AppxManifest.xml
    >
    > PyScriptEngine.exe, a simple host for running Python scripts. <br>
    > See https://github.com/microsoft/AppModelSamples for source.
    >    
    > Installing manifest file:///C:/repos/AppModelSamples/Samples/HostedApps/Python-NumberGuesser/NumberGuesser/AppxManifest.xml...
    >    
    > Success! The app should now appear in the Start Menu.

    (b) To register NumberGuesser as an msix package, first build the msix by right-clicking on the `NumberGuesserPackage` project and choose Publish->Create App Packages. Choose Sideloading and turn off "Enable automatic updates". You can skip package signing, and in the final step be sure to set "Generate app bundle=Never". The output will be a NumberGuesser msix package. Now register this package from the command line:

    > C:\repos\AppModelSamples\Samples\HostedApps\Python-NumberGuesser>pyscriptengine -addpackage .\NumberGuesserProject\AppPackages\NumberGuesserPackage_1.0.0.0_x64_Debug_Test\NumberGuesserPackage_1.0.0.0_x64_Debug.msix -unsigned 
    >
    > PyScriptEngine.exe, a simple host for running Python scripts. <br>
    > See https://github.com/microsoft/AppModelSamples for source. <br>
    > 
    > Allowing unsigned packages.
    > 
    > Installing package file:///C:/repos/AppModelSamples/Samples/HostedApps/Python-NumberGuesser/NumberGuesserProject/AppPackages/NumberGuesserPackage_1.0.0.0_x64_Debug_Test/NumberGuesserPackage_1.0.0.0_x64_Debug.msix...
    > 
    > Success! The app should now appear in the Start Menu.

1. Now, click on "NumberGuesser (Manifest only)" or "NumberGuesser (MSIX)" in your start menu, and run the game!

### Register the Show Headers utility.

1. To register the Show Headers utility, simply point to the AppXManifest file:

    > C:\repos\AppModelSamples\Samples\HostedApps\Python-NumberGuesser>pyscriptengine -register .\ShowHeaders\AppxManifest.xml
    >
    > PyScriptEngine.exe, a simple host for running Python scripts. <br>
    > See https://github.com/microsoft/AppModelSamples for source.
    >    
    > Installing manifest file:///C:/repos/AppModelSamples/Samples/HostedApps/Python-NumberGuesser/ShowHeaders/AppxManifest.xml...
    >    
    > Success! The app should now appear in the Start Menu.

1. Now, find an app that will "Share" a URL and try sharing to the Show Headers app. You can use the Feedback Hub to open any feedback item and share a link from there.
