#pragma once

using namespace winrt;
using namespace Windows::Foundation;
using namespace winrt::Windows::Storage;
using namespace Windows::Foundation::Collections;
using namespace Windows::ApplicationModel;
using namespace Windows::ApplicationModel::Activation;

int main();
void GetDirectories(IVectorView<StorageFolder> folders, hstring searchPattern);
void SearchFile(StorageFile file, hstring searchPattern);
void ShowUsage();

