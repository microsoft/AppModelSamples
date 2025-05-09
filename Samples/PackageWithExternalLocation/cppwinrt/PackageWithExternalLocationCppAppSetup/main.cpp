﻿#include "pch.h"  
#include <iostream>  
#include <string>
#include <winrt/Windows.Management.Deployment.h>  
#include <winrt/Windows.ApplicationModel.Activation.h>  
#include <shellapi.h>

using namespace winrt;  
using namespace winrt::Windows::Management::Deployment;  

void InstallPackage(const std::wstring& packagePath, const std::wstring& externalLocation)
{
    try 
    {
        if (externalLocation.empty() || packagePath.empty())
        {
            MessageBox(nullptr, L"Failed to register app with identity. Please provide the correct absolute path to your package and external location", L"", MB_ICONINFORMATION);
            return;
        }
        
        winrt::Windows::Management::Deployment::PackageManager packageManager;
        winrt::Windows::Management::Deployment::AddPackageOptions addOptions;
        addOptions.ExternalLocationUri(winrt::Windows::Foundation::Uri(winrt::hstring(externalLocation)));

        auto deploymentResult = packageManager.AddPackageByUriAsync(winrt::Windows::Foundation::Uri(winrt::hstring(packagePath)), addOptions).get();
        auto extendedErrorCode = deploymentResult.ExtendedErrorCode();

        if (extendedErrorCode != HRESULT_FROM_WIN32(ERROR_PACKAGES_IN_USE) && FAILED(extendedErrorCode))
        {
            wchar_t buff[10];
            _itow_s(extendedErrorCode, buff, 16);
            if (extendedErrorCode == HRESULT_FROM_WIN32(CERT_E_UNTRUSTEDROOT))
            {
                MessageBox(nullptr, L"Please install your certificate to Trusted People for Local Machine. Running without identity", buff, MB_ICONINFORMATION);
            }
            else
            {
                MessageBox(nullptr, L"Failed to register app with identity. Running without identity", buff, MB_ICONINFORMATION);
            }
        }
    }  
    catch (const std::exception& ex) 
    {
        MessageBoxA(nullptr, "Error during installation: ", ex.what(), MB_ICONERROR);
    }  
}  

void LaunchApplication(const std::wstring& exePath) 
{  
    HINSTANCE result = ShellExecute(nullptr, L"open", exePath.c_str(), nullptr, nullptr, SW_SHOWNORMAL);  
    if ((int)result <= 32) 
    {
        wchar_t buff[10];
        _itow_s((int)result, buff, 16);
        MessageBox(nullptr, L"Failed to launch application", buff, MB_ICONERROR);
    }  
}

void UninstallPackage(const std::wstring& packageFullName)
{  
    winrt::Windows::Management::Deployment::PackageManager packageManager;  
    auto deploymentOperation = packageManager.RemovePackageAsync(packageFullName).get();  
}

int main() 
{
    //TODO - update the value of externalLocation to match the output location of your VS Build binaries even if you want to run without identity.
    //To run with identity, update the value of packagePath to match the path to your signed identity package (.msix). 
    //Note that these values cannot be relative paths and must be complete paths
    std::wstring packagePath = L"";
    std::wstring externalLocation = L"";
    std::wstring packageFullName = L"PackageWithExternalLocationCppSample_1.0.0.0_neutral__h91ms92gdsmmt";

    UninstallPackage(packageFullName); // if the package was not clearly uninstalled, this will remove it
    InstallPackage(packagePath, externalLocation);

    // To launch the application make sure you are passing the correct absolute path to your executable
    if (externalLocation.empty())
    {
        MessageBox(nullptr, L"To launch the application make sure you are passing the correct absolute path to your executable", L"", MB_ICONERROR);
        return -1;
    }
    LaunchApplication(externalLocation + L"\\PackageWithExternalLocationCppApp.exe");
    UninstallPackage(packageFullName);

    return 0;  
}
