using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Management.Deployment;

namespace PyScriptEngine {
    class Program {

        //Declaration to invoke Python3
        [DllImport("python3", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        static extern int Py_Main(int argc, string[] argv);

        static async Task Main(string[] args) {
            bool result = false;

            //Parse command line
            if (args.Length == 2) {
                switch (args[0].ToLower())
                {
                    //Register an appxmanifest.xml file
                    case "-register":
                        if (!File.Exists(args[1])) {
                            Console.WriteLine("Please pass a path to an AppxManifest.xml with the -Register flag");
                        } else {
                            result = await RegisterHostedPackageAsync(args[1]);
                        }
                        break;

                    //Register msix package
                    case "-addpackage":
                        if (!File.Exists(args[1])) {
                            Console.WriteLine("Please pass a path to an MSIX or APPX package with the -AddPackage flag");
                        } else {
                            result = await InstallHostedPackageAsync(args[1]);
                        }
                        break;

                    //We are launched as a hostedapp, process the script!
                    case "-script":
                        Console.WriteLine("Launching script" + args[1]);
                        result = await LaunchHostedPackageAsync(args[1]);
                        break;

                    //switch error
                    default:
                        Console.WriteLine("Unrecognized switch: " + args[0]);
                        break;
                }
            } else {
                Usage();
            }

            Environment.Exit(result ? 0 : -1);

        }

        static async Task<bool> LaunchHostedPackageAsync(string script) {

            //The host 'pyscriptengine' will be running under the identity of the hostedapp 'battleship'
            var scriptFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(script);
            var displayName = Windows.ApplicationModel.Package.Current.DisplayName;
            Console.WriteLine("scriptfile: " + scriptFile.Path);
            await System.Threading.Tasks.Task.Delay(2000); //wait for 2 seconds (= 2000ms)
            return Py_Main(2, new[] { displayName, scriptFile.Path }) == 0;
        }

        static void Usage() {
            var exeName = Path.GetFileName(Assembly.GetEntryAssembly().Location);
            Console.WriteLine(@$"No parameters given. To register a Hosted Package please use: 
{exeName} -AddPackage <Path to myPackage.msix>
 OR
{exeName} -Register <Path to AppxManifest.xml>
from a command line prompt");
        }

        static Task<bool> RegisterHostedPackageAsync(string path) => AddHostedPackageAsync(path, false);
        static Task<bool> InstallHostedPackageAsync(string path) => AddHostedPackageAsync(path, true);

        static async Task<bool> AddHostedPackageAsync(string pathToPackage, bool isPackageFile) {
            var packageUri = new Uri(pathToPackage);

            Console.WriteLine(" Package Address {0}", pathToPackage);
            Console.WriteLine(" Package Uri {0}", packageUri);

            PackageManager packageManager = new PackageManager();

            Task<DeploymentResult> deployment = null;

            // Set AllowUnsigned=true for unsigned Hosted Apps
            if (isPackageFile) {
                var addOptions = new AddPackageOptions { AllowUnsigned = true };
                deployment = packageManager.AddPackageByUriAsync(packageUri, addOptions).AsTask();
            } else {
                var regOptions = new RegisterPackageOptions { AllowUnsigned = true };
                deployment = packageManager.RegisterPackageByUriAsync(packageUri, regOptions).AsTask();
            }

            Console.WriteLine("Installing package {0}", packageUri);
            Debug.WriteLine("Waiting for package registration to complete...");

            try {
                await deployment;
                Console.WriteLine("Registration succeeded! Try running the new app.");
                return true;
            } catch (OperationCanceledException) {
                Console.WriteLine("Registration was canceled");
            } catch (AggregateException ex) {
                Console.WriteLine($"Installation Error: {ex}");
            }
            return false;
        }
    }
}
