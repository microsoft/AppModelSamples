using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace PyScriptEngine
{
    class Program
    {
        private static class NativeMethods
        {
            // Declaration to invoke Python3
            [DllImport("python3", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            internal static extern int Py_Main(int argc, string[] argv);
        }

        static async Task<int> Main(string[] args)
        {
            bool result = false;

            if (args.Length != 2)
            {
                Usage();
                return 1;
            }

            string command = args[0].ToLowerInvariant();
            string param = args[1];

            // Parse the command line
            switch (command)
            {
                // Register an AppxManifest.xml file
                case "-register":
                    if (!File.Exists(param) || Path.GetFileName(param).ToLowerInvariant() != "appxmanifest.xml")
                    {
                        Console.Error.WriteLine($"File '{param}' does not exist or is not an AppXManifest.xml file.");
                    }
                    else
                    {
                        result = await RegisterHostedPackageAsync(param);
                    }
                    break;

                // Register an MSIX package
                case "-addpackage":
                    var extension = Path.GetExtension(param).ToLowerInvariant();
                    if (!File.Exists(param) || (extension != "msix" && extension != "appx"))
                    {
                        Console.Error.WriteLine($"File '{param}' does not exist or is not a valid MSIX file.");
                    }
                    else
                    {
                        result = await InstallHostedPackageAsync(param);
                    }
                    break;

                // Run the provided script
                case "-script":
                    result = LaunchHostedPackage(param);
                    break;

                // Unknown error
                default:
                    Console.Error.WriteLine($"Unrecognized switch: {command}.");
                    break;
            }

            return result ? 0 : 2;
        }

        static bool LaunchHostedPackage(string filename)
        {
            // The host 'pyscriptengine' will be running under the identity of the hosted app
            var scriptFullName = Path.Combine(Package.Current.InstalledPath, filename);
            var displayName = Package.Current.DisplayName;

            Console.WriteLine($"Launching script: {scriptFullName}...");
            Console.WriteLine();

            return NativeMethods.Py_Main(2, new[] { displayName, scriptFullName }) == 0;
        }

        static void Usage()
        {
            var exeName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            Console.WriteLine(@$"
Usage:
  
  To register an MSIX package, use:  {exeName} -AddPackage <MSIX-file>
  To register a loose package, use:  {exeName} -Register <AppXManifest.xml>
  
  To run a registered package, run it's associated tile from the Start menu.
");
        }

        static Task<bool> RegisterHostedPackageAsync(string filename) => AddHostedPackageAsync(filename, false);
        static Task<bool> InstallHostedPackageAsync(string filename) => AddHostedPackageAsync(filename, true);

        static async Task<bool> AddHostedPackageAsync(string filename, bool isPackageFile)
        {
            var packageUri = new Uri(Path.GetFullPath(filename));
            PackageManager packageManager = new PackageManager();
            Task<DeploymentResult> deployment = null;

            Console.WriteLine($"Installing package {packageUri}...");

            if (isPackageFile)
            {
                // Package files are always signed, by definition.
                var addOptions = new AddPackageOptions();
                deployment = packageManager.AddPackageByUriAsync(packageUri, addOptions).AsTask();
            }
            else
            {
                // AppXManifest is not signed
                var regOptions = new RegisterPackageOptions { AllowUnsigned = true };
                deployment = packageManager.RegisterPackageByUriAsync(packageUri, regOptions).AsTask();
            }

            try
            {
                await deployment;
                Console.WriteLine("Installation succeeded. The app should appear in the Start menu now.");
                return true;
            }
            catch (OperationCanceledException)
            {
                Console.Error.WriteLine("Installation was canceled.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Installation Error: {ex.Message}.");
            }

            Console.WriteLine();
            Console.WriteLine("Installation failed.");
            return false;
        }
    }
}
