This repo contains 2 applications, both of which requires Java to run. One on them is a **jar** file and another executes java.exe to get its version. This will demonstrate how to package java and use it in the 2 scenarios.

## Instructions
You can learn more about [hosted apps](https://learn.microsoft.com/en-us/windows/uwp/launch-resume/hosted-apps).

### Minimum OS Requirements:
1. Windows 10 20H1 (10.0.19041.0)

### Steps to package Java
1. Get Java on your machine. You can download it from [openJDK site](https://jdk.java.net/20/) and then extract the contents.
2. Copy the [sample java manifest](java_sample_manifest.xml) to inside the extracted folder as AppxManifest.xml.
3. Run makeappx to create the package

```powershell
Invoke-WebRequest -Uri https://download.java.net/java/GA/jdk20.0.1/b4887098932d415489976708ad6d1a4b/9/GPL/openjdk-20.0.1_windows-x64_bin.zip -OutFile $env:TEMP\jdk_20.zip
Expand-Archive -Path $env:TEMP\jdk_20.zip -DestinationPath $env:TEMP\jdk_20
Copy-Item -Path .\java_sample_manifest.xml $env:TEMP\jdk_20\jdk-20.0.1\AppxManifest.xml
makeappx.exe pack /nv /nfv /d $env:TEMP\jdk_20\jdk-20.0.1 /p openjdk_20.msix /o
```

### Steps to run jar application
1. Build the [JarApplication](JarApplication) project. See [Readme](JarApplication/README.md) on how to create an MSIX
2. Install *packaged java* on the system and then the *packaged application* (java_version.msix)
3. Now you should be able to launch app (Java Version) and it should display the version of java it is using.


### Steps to run Application requiring java
1. Build the [AppDependsOnJava](AppDependsOnJava) project. See [Readme](AppDependsOnJava/README.md) on how to create the MSIX
2. Install *packaged java* on the system and then the *packaged application* 
3. Now you should be able to launch the app (AppDependsOnJava) and it should display the version of java it is using.

