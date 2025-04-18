#include "pch.h"

#include <winrt/Windows.Management.Deployment.h>
#include <winrt/Windows.ApplicationModel.Activation.h>

using namespace winrt;
using namespace winrt::Windows::Management::Deployment;
using namespace winrt::Windows::ApplicationModel::Activation;

#pragma comment(lib, "mfplat.lib")
#pragma comment(lib, "mf.lib")
#pragma comment(lib, "mfreadwrite.lib")
#pragma comment(lib, "evr.lib")
#pragma comment(lib, "d3d9.lib")

using namespace Microsoft::WRL;

IMFMediaSession* pSession = nullptr;
IMFMediaSource* pSource = nullptr;
IMFActivate* pActivate = nullptr;
IMFVideoDisplayControl* pVideoDisplay = nullptr;
HWND hwndVideo = nullptr;

HRESULT InitializeWebcam(HWND hwnd) 
{
    IMFAttributes* pAttributes = nullptr;
    IMFActivate** ppDevices = nullptr;
    UINT32 count = 0;

    // Create an attribute store to specify the video capture device
    HRESULT hr = MFCreateAttributes(&pAttributes, 1);
    if (FAILED(hr)) return hr;

    hr = pAttributes->SetGUID(MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE, MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID);
    if (FAILED(hr)) return hr;

    // Enumerate video capture devices
    hr = MFEnumDeviceSources(pAttributes, &ppDevices, &count);
    if (FAILED(hr)) return hr;

    if (count == 0) 
    {
        MessageBox(hwnd, L"No webcam found.", L"Error", MB_ICONERROR);
        return E_FAIL;
    }

    // Use the first device
    pActivate = ppDevices[0];
    for (UINT32 i = 1; i < count; i++) 
    {
        ppDevices[i]->Release();
    }
    CoTaskMemFree(ppDevices);
    pAttributes->Release();

    // Create the media source for the webcam
    hr = pActivate->ActivateObject(__uuidof(IMFMediaSource), (void**)&pSource);
    if (FAILED(hr)) return hr;

    // Create a media session
    hr = MFCreateMediaSession(nullptr, &pSession);
    if (FAILED(hr)) return hr;

    // Create a topology for the webcam
    IMFTopology* pTopology = nullptr;
    hr = MFCreateTopology(&pTopology);
    if (FAILED(hr)) return hr;

    IMFPresentationDescriptor* pPD = nullptr;
    hr = pSource->CreatePresentationDescriptor(&pPD);
    if (FAILED(hr)) return hr;

    DWORD streamCount = 0;
    pPD->GetStreamDescriptorCount(&streamCount);

    for (DWORD i = 0; i < streamCount; i++) 
    {
        BOOL selected = FALSE;
        IMFStreamDescriptor* pSD = nullptr;
        hr = pPD->GetStreamDescriptorByIndex(i, &selected, &pSD);
        if (FAILED(hr)) continue;

        if (selected) 
        {
            IMFTopologyNode* pSourceNode = nullptr;
            IMFTopologyNode* pOutputNode = nullptr;

            // Create a source node
            hr = MFCreateTopologyNode(MF_TOPOLOGY_SOURCESTREAM_NODE, &pSourceNode);
            if (FAILED(hr)) continue;

            hr = pSourceNode->SetUnknown(MF_TOPONODE_SOURCE, pSource);
            if (FAILED(hr)) continue;

            hr = pSourceNode->SetUnknown(MF_TOPONODE_PRESENTATION_DESCRIPTOR, pPD);
            if (FAILED(hr)) continue;

            hr = pSourceNode->SetUnknown(MF_TOPONODE_STREAM_DESCRIPTOR, pSD);
            if (FAILED(hr)) continue;

            // Create an output node
            hr = MFCreateTopologyNode(MF_TOPOLOGY_OUTPUT_NODE, &pOutputNode);
            if (FAILED(hr)) continue;

            IMFActivate* pRendererActivate = nullptr;
            hr = MFCreateVideoRendererActivate(hwnd, &pRendererActivate);
            if (FAILED(hr)) continue;

            hr = pOutputNode->SetObject(pRendererActivate);
            if (FAILED(hr)) continue;

            // Add nodes to the topology
            hr = pTopology->AddNode(pSourceNode);
            if (FAILED(hr)) continue;

            hr = pTopology->AddNode(pOutputNode);
            if (FAILED(hr)) continue;

            hr = pSourceNode->ConnectOutput(0, pOutputNode, 0);
            if (FAILED(hr)) continue;

            pSourceNode->Release();
            pOutputNode->Release();
        }
        pSD->Release();
    }

    hr = pSession->SetTopology(0, pTopology);
    if (FAILED(hr)) return hr;

    pTopology->Release();
    pPD->Release();

    // Start the session
    PROPVARIANT varStart;
    PropVariantInit(&varStart);
    hr = pSession->Start(&GUID_NULL, &varStart);
    PropVariantClear(&varStart);

    return hr;
}

void Cleanup() 
{
    if (pSession) 
    {
        pSession->Close();
        pSession->Release();
    }
    if (pSource) pSource->Release();
    if (pActivate) pActivate->Release();
    MFShutdown();
}

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam) 
{
    switch (msg) 
    {
    case WM_DESTROY:
        Cleanup();
        PostQuitMessage(0);
        break;
    }
    return DefWindowProc(hwnd, msg, wParam, lParam);
}

HRESULT RegisterPackageWithExternalLocation(const std::wstring& externalLocation, const std::wstring& packagePath)
{ 
    winrt::Windows::Management::Deployment::PackageManager packageManager;  
    winrt::Windows::Management::Deployment::AddPackageOptions addOptions;  
    addOptions.ExternalLocationUri(winrt::Windows::Foundation::Uri(winrt::hstring(externalLocation)));
    auto result = packageManager.AddPackageByUriAsync(winrt::Windows::Foundation::Uri(winrt::hstring(packagePath)), addOptions).get();

    return static_cast<HRESULT>(result.ExtendedErrorCode().value);
}

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE, LPSTR, int nCmdShow) 
{

    //TODO - update the value of externalLocation to match the output location of your VS Build binaries and the value of 
    //packagePath to match the path to your signed Sparse Package (.msix). 
    //Note that these values cannot be relative paths and must be complete paths
    std::wstring externalLocation = L"";
    std::wstring packagePath = L"";

    //Attempt registration
    if (!externalLocation.empty() && !packagePath.empty())
    {
        auto result = RegisterPackageWithExternalLocation(externalLocation, packagePath);
        if (result != HRESULT_FROM_WIN32(ERROR_PACKAGES_IN_USE) && FAILED(result))
        {
            if (result == HRESULT_FROM_WIN32(CERT_E_UNTRUSTEDROOT))
            {
                MessageBox(nullptr, L"Please install your certificate to Trusted Root Certification Authorities for Local Machine. Running without identity", L"result", MB_ICONINFORMATION);
            }
            else
            {
                MessageBox(nullptr, L"Failed to register app with identity. Running without identity", L"result", MB_ICONINFORMATION);
            }
        }
    }

    MFStartup(MF_VERSION);

    // Register window class
    WNDCLASS wc = {};
    wc.lpfnWndProc = WndProc;
    wc.hInstance = hInstance;
    wc.lpszClassName = L"WebcamApp";

    RegisterClass(&wc);

    // Create window
    hwndVideo = CreateWindowEx(0, wc.lpszClassName, L"Webcam Viewer", WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT, 800, 600, nullptr, nullptr, hInstance, nullptr);

    if (!hwndVideo) return -1;

    ShowWindow(hwndVideo, nCmdShow);

    // Initialize webcam
    if (FAILED(InitializeWebcam(hwndVideo))) 
    {
        MessageBox(nullptr, L"Failed to initialize webcam. Please check if app is running with identity. If not, turn on camera usage for desktop app in Settings and relaunch.", L"Error", MB_ICONERROR);
        return -1;
    }

    // Main message loop
    MSG msg = {};
    while (GetMessage(&msg, nullptr, 0, 0)) 
    {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    return 0;
}

void RemovePackageWithExternalLocation() // example of how to uninstall a package with external location
{
    winrt::Windows::Management::Deployment::PackageManager packageManager;
    auto deploymentOperation{ packageManager.RemovePackageAsync(L"PackageWithExternalLocationCppSample_1.0.0.0_neutral__h91ms92gdsmmt") };
    deploymentOperation.get();
}
