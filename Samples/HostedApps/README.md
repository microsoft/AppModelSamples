This repo contains 2 sample applications demonstrating how to use the Hosted AppModel to define a host and a hosted app.

## Instructions
You can learn more about the Hosted AppModel in this documentation.

### Requirements:
1. Windows SDK version 10.0.19563.0 or higher
2. Windows OS version 10.0.19563.0 or higher

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
1. Retarget the solution to the 10.0.19563 or higher SDK version on your machine – Right click -> Retarget solution.
1. Open WinformsToastHost.sln in VS2019.
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

## Python host & Number Guesser game
In this example, the host is comprised of 2 projects – first is PyScriptEngine which is wrapper written in C# and makes use of the Python nuget package to run python scripts. 
This wrapper parses the command line and has the capability to register a manifest as well as launch the python executable with a path to a script file. 
The second project is PyScriptEnginePackage which is a Windows Application Packaging Project that installs PyScriptEngine and registers the manifest that includes the HostRuntime extension.
### NumberGuesser
The Hosted App is made up of a python script and visual assets. It doesn’t contain any PE files. 
It includes an application manifest where the declarations for HostRuntimeDependency and HostId are declared that identifies PyScriptEngine as its Host. 
The manifest also contains the Unsigned Publisher OID entry that is required for an unsigned package.

### Building and running the sample
1. Make sure your machine has Developer Mode turned on.
2. Retarget the solution to the SDK version on your machine – Right click -> Retarget solution.
3. Open PyScriptEngine.sln solution in Visual Studio
4. Set PyScriptEnginePackage as the Startup project
5. Build PyScriptEnginePackage 
6. Deploy PyScriptEnginePackage  (Right-Click PyScriptEnginePackage | Deploy) 
7. Because the host app declares an appexecutionalias, you now go to a command prompt and run “pyscriptengine” to get the usage notice:

> D:\repos\HostedApps>pyscriptengine <br>
> No parameters given. To register a Hosted Package please use: <br>
> PyScriptEngine.dll -AddPackage <Path to myPackage.msix> <br>
> &nbsp;&nbsp;OR <br>
> PyScriptEngine.dll -Register <Path to AppxManifest.xml> <br>
> from a command line prompt <br>

8.	Now register the hosted application
> D:\repos\HostedApps>pyscriptengine -Register D:\repos\HostedApps\NumberGuesser\AppxManifest.xml <br>
> Package Address D:\repos\HostedApps\NumberGuesser\AppxManifest.xml <br>
> Package Uri file:///D:/repos/HostedApps/NumberGuesser/AppxManifest.xml <br>
> Installing package file:///D:/repos/HostedApps/NumberGuesser/AppxManifest.xml <br>
> Registration succeeded! Try running the new app. <br>

9.	Now, click on "NumberGuesser" in your start menu, and run the game!


