using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace CsFindstr
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Insufficient arguments.");
                Console.WriteLine("Usage:");
                Console.WriteLine("   mFindstr <search-pattern> <fully-qualified-folder-path>.");
                Console.WriteLine("Example:");
                Console.WriteLine("   mFindstr on D:\\Temp.");
            }
            else
            {
                string searchPattern = args[0];
                string folderPath = args[1];
                RecurseFolders(folderPath, searchPattern).Wait();
            }

            Console.WriteLine("Press a key to continue: ");
            Console.ReadLine();
        }

        private static async Task<bool> RecurseFolders(string folderPath, string searchPattern)
        {
            bool success = true;
            try
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(folderPath);

                if (folder != null)
                {
                    Console.WriteLine($"Searching folder '{folder.Name}' and below for pattern '{searchPattern}'");
                    try
                    {
                        // Get the files in this folder.
                        IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                        foreach (StorageFile file in files)
                        {
                            SearchFile(file, searchPattern);
                        }

                        // Recurse sub-directories.
                        IReadOnlyList<StorageFolder> subDirs = await folder.GetFoldersAsync();
                        if (subDirs.Count != 0)
                        {
                            GetDirectories(subDirs, searchPattern);
                        }
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                Console.WriteLine(ex.Message);
            }
            return success;
        }

        private static async void GetDirectories(IReadOnlyList<StorageFolder> folders, string searchPattern)
        {
            try
            {
                foreach (StorageFolder folder in folders)
                {
                    // Get the files in this folder.
                    IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                    foreach (StorageFile file in files)
                    {
                        SearchFile(file, searchPattern);
                    }

                    // Recurse this folder to get sub-folder info.
                    IReadOnlyList<StorageFolder> subDirs = await folder.GetFoldersAsync();
                    if (subDirs.Count != 0)
                    {
                        GetDirectories(subDirs, searchPattern);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async void SearchFile(StorageFile file, string searchPattern)
        {
            if (file != null)
            {
                try
                {
                    Console.WriteLine($"Scanning file '{file.Path}'");
                    string text = await FileIO.ReadTextAsync(file);
                    string compositePattern = "(\\S+\\s+){0}\\S*" + searchPattern + "\\S*(\\s+\\S+){0}";
                    Regex regex = new Regex(compositePattern);
                    MatchCollection matches = regex.Matches(text);
                    foreach (Match match in matches)
                    {
                        Console.WriteLine($"{match.Index,8} {match.Value}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}

