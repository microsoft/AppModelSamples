This repo contains 2 applications, shared package container definition XML and source code of the app. These will help in demonstrating 
how to use shared package container feature of MSIX.

## Instructions
You can learn more about MSIX Shared Package Container in this [documentation](https://learn.microsoft.com/en-us/windows/msix/manage/shared-package-container).

### Minimum OS Requirements:
1. Windows 10 Insider Preview Build 21354

### Explanation 
There are two apps. One without image (app-without-image-setup_1.0.0.0_x64__8h66172c634n0.msix) and 
other with image (app-with-image-setup_1.0.0.0_x64__8h66172c634n0.msix)

By default, without shared package container, "app-without-image-setup" would not be able to load image on click of button "click" as image is not present in the VFS folder of this app.

The shared package container defintion when deployed would help the first app (app-without-image-setup_1.0.0.0_x64__8h66172c634n0.msix) load image successfully as there will be a shared runtime container present in the system for these two apps. Using the shared runtime container, "app-without-image-setup" will be able to use the image present in VFS folder of "app-with-image-setup" as VFS folders of both the apps will be merged when the "app-without-image-setup" is launched.
### 
Note: The shared package container definition XML and the two apps need to be deployed in the system for this scenario to execute.

### Steps
#### 1. Deploy Shared package container definition
Add-AppSharedPackageContainer ".\sp-container-definition.xml"
#### 2. Install the MSIX packages
app-without-image-setup_1.0.0.0_x64__8h66172c634n0.msix
app-with-image-setup_1.0.0.0_x64__8h66172c634n0.msix
#### 3. Launch app-without-image-setup
#### 4. Click on "Click" button and see the app load image successfully.
