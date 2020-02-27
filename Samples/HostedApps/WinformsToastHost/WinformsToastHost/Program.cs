using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using Windows.ApplicationModel;

namespace WinformsToastHost
{
    static class Program
    {
        // Runtime identity details - normally an app has a fixed identity, but
        // when running as a host each instance will run in the context of the 
        // hosted app (not the context of the host's package).
        public static string PackageId { get; }
        public static string PackageDirectory { get; }

        static Program()
        {
            try
            {
                // Call Windows packaging APIs to get identity information
                Package package = Package.Current;
                PackageId = package.Id.FullName;
                PackageDirectory = package.InstalledLocation.Path.ToLowerInvariant();
                HasPackageIdentity = true;
            }

            // App is running without being packaged at all
            catch (Exception ex) when (ex.HResult == -2147009196)
            {
                PackageId = "Not packaged";
                PackageDirectory = "Not packaged";
                HasPackageIdentity = false;
            }
        }

        public static string ExecutableDirectory => Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName).ToLowerInvariant();

        public static bool HasPackageIdentity { get; }

        // If the package path is the same as the EXE path, then the app is not running
        // as a host (it is running "as itself")
        public static bool IsRunningAsHost
        {
            get
            {
                return HasPackageIdentity && !(ExecutableDirectory.StartsWith(PackageDirectory));
            }
        }

        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
