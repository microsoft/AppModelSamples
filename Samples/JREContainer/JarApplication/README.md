## Instructions
Build this project to generate a msix package containing a jar file.


### Requirements
1. [java](https://jdk.java.net/20/)
2. [maven](https://maven.apache.org/download.cgi)

### Building msix
1. Compile the project using maven
2. Generate jar file using maven
3. Bundle the jar into a msix

```powershell
mvn clean compile
mvn package
New-Item -Path .\target\temp -ItemType "directory"
Copy-Item -Path .\target\version-0.1.0.jar .\target\temp\version.jar
Copy-Item -Path .\AppxManifest.xml .\target\temp
makeappx.exe pack /nv /nfv /d .\target\temp /p .\target\java_version.msix /o
```
