using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Management.Deployment;

namespace PyScriptEngine
{
    class Program
    {
        private static class NativeMethods
        {
            // Export from the included Python nuget package
            [DllImport("python3", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint ="Py_Main")]
            internal static extern int RunPythonScript(int argc, string[] argv);
        }

        static string ExeName { get; } = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);

        static async Task<int> Main(string[] args)
        {
            Console.WriteLine($"{ExeName}, a simple host for running Python scripts.");
            Console.WriteLine("See https://github.com/microsoft/AppModelSamples for source.");
            Console.WriteLine();

            bool result = false;

            // Check for Share operations -- we only support WebLink (URLs) in this sample.
            List<string> scriptArguments = new List<string>();
            var activatedArgs = AppInstance.GetActivatedEventArgs();
            if (activatedArgs != null && activatedArgs.Kind == Windows.ApplicationModel.Activation.ActivationKind.ShareTarget)
            {
                var shareArgs = activatedArgs as ShareTargetActivatedEventArgs;
                var dataPackage = shareArgs.ShareOperation.Data;
                if (dataPackage.AvailableFormats.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.WebLink))
                {
                    var uri = (await dataPackage.GetWebLinkAsync()).AbsoluteUri;
                    scriptArguments.Add(uri);
                    shareArgs.ShareOperation.ReportCompleted();

                    // Share operations drop the command-line args from the manifest, so we have to 
                    // re-create them.
                    args = FixupArgsForShare();
                }
            }

            if ((args.Length != 2) && (args.Length != 3))
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
                    if (!File.Exists(param) || (extension != ".msix" && extension != ".appx"))
                    {
                        Console.Error.WriteLine($"File '{param}' does not exist or is not a valid MSIX file.");
                    }
                    else
                    {
                        bool allowUnsigned = false;

                        if (args.Length >= 3)
                        {
                            var extraParam = args[2].ToLowerInvariant();
                            if (extraParam == "-unsigned")
                            {
                                allowUnsigned = true;
                            }
                        }

                        Console.WriteLine($"{(allowUnsigned ? "Allowing" : "Not allowing")} unsigned packages.");
                        result = await InstallHostedPackageAsync(param, allowUnsigned);
                    }
                    break;

                // Run the provided script
                case "-script":
                    result = LaunchHostedPackage(param, scriptArguments);
                    break;

                // Unknown error
                default:
                    Console.Error.WriteLine($"Unrecognized switch: {command}.");
                    break;
            }

            return result ? 0 : 2;
        }

        // When launched as a Share Target, we don't get command-line args, so we will go probing
        // for the script to launch instead. This sample blindly calls the first python script it
        // finds; another option would be to have a naming convention like "hosted_app.py"
        private static string[] FixupArgsForShare()
        {
            var filenames = Directory.GetFiles(Package.Current.InstalledPath, "*.py");
            if (filenames.Length == 0)
            {
                return new string[0];
            }

            return new[] { "-Script", filenames[0] };
        }

        static bool LaunchHostedPackage(string filename, IList<string> scriptArguments)
        {
            // The host 'pyscriptengine' will be running under the identity of the hosted app
            var scriptFullName = Path.Combine(Package.Current.InstalledPath, filename);
            var displayName = Package.Current.DisplayName;

            scriptArguments.Insert(0, displayName);
            scriptArguments.Insert(1, scriptFullName);

            Console.WriteLine($"Launching script: {scriptFullName}...");
            Console.WriteLine();

            return NativeMethods.RunPythonScript(scriptArguments.Count, scriptArguments.ToArray()) == 0;
        }

        static void Usage()
        {
            Console.WriteLine(@$"
Usage:
  
  To register a loose package:

    {ExeName} -Register <AppXManifest.xml>

  To register an MSIX package:

    {ExeName} -AddPackage <MSIX-file> [-unsigned]

    The optional -unsigned parameter is used if the package is unsigned. 
    In this case, the package cannot include any executable files; only 
    content files (like .py scripts or images) for the Host to execute.

  To run a registered package, run it from the Start Menu.
");
        }

        static Task<bool> RegisterHostedPackageAsync(string filename) => AddHostedPackageAsync(filename, false, true);
        static Task<bool> InstallHostedPackageAsync(string filename, bool allowUnsigned) => AddHostedPackageAsync(filename, true, allowUnsigned);

        static async Task<bool> AddHostedPackageAsync(string filename, bool isPackageFile, bool allowUnsigned)
        {
            var packageUri = new Uri(Path.GetFullPath(filename));
            PackageManager packageManager = new PackageManager();
            Task<DeploymentResult> deployment = null;

            Console.WriteLine();
            Console.WriteLine($"Installing {(isPackageFile ? "package" : "manifest")} {packageUri}...");

            if (isPackageFile)
            {
                // Package files that contain code must be signed, but those only containing "content" (e.g.
                // packages that rely on a host for their executables) do not need to be signed. 
                var addOptions = new AddPackageOptions();
                addOptions.AllowUnsigned = allowUnsigned;
                deployment = packageManager.AddPackageByUriAsync(packageUri, addOptions).AsTask();
            }
            else
            {
                // Raw AppXManifest is not signed; always allow unsigned things
                var regOptions = new RegisterPackageOptions { AllowUnsigned = true };
                deployment = packageManager.RegisterPackageByUriAsync(packageUri, regOptions).AsTask();
            }

            try
            {
                await deployment;
                Console.WriteLine();
                Console.WriteLine("Success! The app should now appear in the Start Menu.");
                return true;
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine($"Installation Error: {ex.Message}.");
            }

            Console.WriteLine();
            Console.WriteLine("Installation failed.");
            return false;
        }
    }
}
