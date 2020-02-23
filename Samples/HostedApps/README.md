This repo contains 2 sample applications demonstrating how to use the Hosted AppModel to define a host and a hosted app.

## Instructions
You can learn more about the Hosted AppModel in this documentation.

### Requirements:
1. Windows SDK version 10.0.19041.0 or higher
2. Windows OS version 10.0.19041.0 or higher

## WinformsToastHost with binary extension
The host in this example is a simple Windows Forms application that displays its package identity, location, and calls the ToastNotification apis. 
It also has the capability to load a binary extension from a hosted app package through reflection. 
When run under its own identity, it does not display the extension information. 
The application is packaged with the Windows Application Packaging Project which includes the manifest declarations for being a host.

### WinformsToastHost-Extension
The hosted app is a .NET dll that implements an extension mechanism for the host to load. 
It also includes a packaging project that declares its identity and dependency upon the hostruntime. 
You will see this identity reflected in the values displayed when the application is run. 
When registered, the host has access to the hostedapp’s package location and thus can load the extension. 

### Building and running the sample
1. Make sure your machine has Developer Mode turned on.
2. Retarget the solution to the 10.0.19041 or higher SDK version on your machine – Right click -> Retarget solution.
3. Open WinformsToastHost.sln in VS2019
4. Build and deploy WinformsToastHost.Package
5. Build and deploy HostedExtension
6. Goto Start menu and launch WinformsToastHost
7. Goto Start menu and launch WinformsToastHost-HostedExtension

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


