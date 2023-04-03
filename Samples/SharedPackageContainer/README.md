This repo contains 2 customized VLC media player applications and a shared package container definition XML that will help in demonstrating 
how to use shared package container feature of MSIX.

## Instructions
You can learn more about MSIX Shared Package Container in this [documentation](https://learn.microsoft.com/en-us/windows/msix/manage/shared-package-container).

### Minimum OS Requirements:
1. Windows 10 Insider Preview Build 21354

### Explanation
Plugin is an essential part of VLC media player app and is needed for it to run. 
So, there are two customized apps in this section. One without plugin (app-without-plugin_1.0.0.0_x64__8h66172c634n0.msix) and 
other with plugin (app-with-plugin_1.0.0.0_x64__8h66172c634n0.msix)

The shared package container defintion when deployed would help the first version (app-without-plugin_1.0.0.0_x64__8h66172c634n0.msix) which is 
without plugin run successfully as there will be a shared runtime container present in the system for these two packages.
### Note: 
The shared package container definition XML and the two customized apps need to be deployed in the system for this scenario to execute.
