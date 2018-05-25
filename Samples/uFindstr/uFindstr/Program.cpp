//*********************************************************  
//  
// Copyright (c) Microsoft. All rights reserved.  
// This code is licensed under the MIT License (MIT).  
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF  
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY  
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR  
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.  
//  
//*********************************************************  

#include "pch.h"
#include "Program.h"

int main()
{
	if (__argc < 3)
	{
		ShowUsage();
		return 1;
	}

	hstring searchPattern = to_hstring(__argv[1]);
	hstring folderPath = to_hstring(__argv[2]);

	StorageFolder folder = nullptr;
	try
	{
		folder = StorageFolder::GetFolderFromPathAsync(folderPath.c_str()).get();
	}
	catch (...)
	{
		wprintf(L"Error: cannot access folder '%S'\n", __argv[2]);
		return 2;
	}

	if (folder != nullptr)
	{
		wprintf(L"\nSearching folder '%s' and below for pattern '%s'\n", folder.Path().c_str(), searchPattern.c_str());
		try
		{
			IVectorView<StorageFolder> folders = folder.GetFoldersAsync().get();

			// Recurse sub-directories.
			GetDirectories(folders, searchPattern);

			// Get the files in this folder.
			IVectorView<StorageFile> files = folder.GetFilesAsync().get();
			for (uint32_t i = 0; i < files.Size(); i++)
			{
				StorageFile file = files.GetAt(i);
				SearchFile(file, searchPattern);
			}
		}
		catch (std::exception ex)
		{
			wprintf(L"Error: %S\n", ex.what());
			return 3;
		}
		catch (...)
		{
			wprintf(L"Error: unknown\n");
			return 3;
		}
	}

	return 0;
}

void ShowUsage()
{
	wprintf(L"Error: insufficient arguments.\n");
	wprintf(L"Usage:\n");
	wprintf(L"ufindstr <search-pattern> <fully-qualified-folder-path>.\n");
	wprintf(L"Example:\n");
	wprintf(L"ufindstr on D:\\Temp.\n");

	wprintf(L"\nPress Enter to continue:");
	getchar();
}

void GetDirectories(IVectorView<StorageFolder> folders, hstring searchPattern)
{
	try
	{
		for (uint32_t i = 0; i < folders.Size(); i++)
		{
			StorageFolder folder = folders.GetAt(i);

			// Recurse this folder to get sub-folder info.
			IVectorView<StorageFolder> subDir = folder.GetFoldersAsync().get();
			if (subDir.Size() != 0)
			{
				GetDirectories(subDir, searchPattern);
			}

			// Get the files in this folder.
			IVectorView<StorageFile> files = folder.GetFilesAsync().get();
			for (uint32_t j = 0; j < files.Size(); j++)
			{
				StorageFile file = files.GetAt(j);
				SearchFile(file, searchPattern);
			}
		}
	}
	catch (std::exception ex)
	{
		wprintf(L"Error: %S\n", ex.what());
	}
	catch (...)
	{
		wprintf(L"Error: unknown\n");
	}
}

void SearchFile(StorageFile file, hstring searchPattern)
{
	if (file != nullptr)
	{
		try
		{
			wprintf(L"\nScanning file '%s'\n", file.Path().c_str());
			hstring text = FileIO::ReadTextAsync(file).get();
			std::string sourceText = to_string(text);
			std::smatch match;
			std::string compositePattern = "(\\S+\\s+){0}\\S*" + to_string(searchPattern) + "\\S*(\\s+\\S+){0}";
			std::regex expression(compositePattern);

			while (std::regex_search(sourceText, match, expression))
			{
				wprintf(L"%8d %S\n", match.position(), match[0].str().c_str());
				sourceText = match.suffix().str();
			}
		}
		catch (std::exception ex)
		{
			wprintf(L"Error: %S\n", ex.what());
		}
		catch (...)
		{
			wprintf(L"Error: cannot read text from file.\n");
		}
	}
}

