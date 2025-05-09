﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Foundation.Collections;
using Windows.Management.Deployment;
using Windows.Storage;

namespace PhotoStoreDemo
{
    public class StartUp
    {
        [STAThread]
        public static void Main(string[] cmdArgs)
        {
            //if app isn't running with identity, register its identity package
            if (!ExecutionMode.IsRunningWithIdentity())
            {
                RegisterIdentityAndRelaunchAsync(cmdArgs).GetAwaiter().GetResult();
            }
            else //App is registered and running with identity, handle launch and activation
            {
                //Handle identity package based activation e.g Share target activation or clicking on a Tile
                // Launching the .exe directly will have activationArgs == null
                var activationArgs = AppInstance.GetActivatedEventArgs();
                if (activationArgs != null)
                {
                    switch (activationArgs.Kind)
                    {
                        case ActivationKind.Launch:
                            HandleLaunch(activationArgs as LaunchActivatedEventArgs);
                            break;
                        case ActivationKind.ToastNotification:
                            HandleToastNotification(activationArgs as ToastNotificationActivatedEventArgs);
                            break;
                        case ActivationKind.ShareTarget:
                            HandleShareAsync(activationArgs as ShareTargetActivatedEventArgs);
                            break;
                        default:
                            HandleLaunch(null);
                            break;
                    }

                }
                //This is a direct exe based launch e.g. double click app .exe or desktop shortcut pointing to .exe
                else
                {
                    SingleInstanceManager singleInstanceManager = new SingleInstanceManager();
                    singleInstanceManager.Run(cmdArgs);
                }
            }
        }

        static async Task RegisterIdentityAndRelaunchAsync(string[] cmdArgs)
        {
            //TODO - update the value of externalLocation to match the output location of your VS Build binaries and the value of 
            //packagePath to match the path to your signed identity package (.msix). 
            //Note that these values cannot be relative paths and must be complete paths
            string externalLocation = @"";
            string packagePath = @"";

            //Attempt registration
            if (await RegisterPackageWithExternalLocationAsync(externalLocation, packagePath))
            {
                //Registration succeeded, restart the app to run with identity
                Process.Start(Application.ResourceAssembly.Location, arguments: cmdArgs?.ToString());
            }
            else //Registration failed, run without identity
            {
                Debug.WriteLine("Package Registation failed, running WITHOUT Identity");
                SingleInstanceManager wrapper = new SingleInstanceManager();
                wrapper.Run(cmdArgs);
            }
        }

        static void HandleLaunch(LaunchActivatedEventArgs args)
        {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Debug.AutoFlush = true;
            Debug.Indent();
            Debug.WriteLine("WPF App using an identity package");


            SingleInstanceManager singleInstanceManager = new SingleInstanceManager();
            singleInstanceManager.Run(Environment.GetCommandLineArgs());
        }

        static void HandleToastNotification(ToastNotificationActivatedEventArgs args)
        {
            ValueSet userInput = args.UserInput;
            string pathFromToast = userInput["textBox"].ToString();

            ImageFile item = new ImageFile(pathFromToast);
            item.AddToCache();

            SingleInstanceManager singleInstanceManager = new SingleInstanceManager();
            singleInstanceManager.Run(Environment.GetCommandLineArgs());
        }

        static async void HandleShareAsync(ShareTargetActivatedEventArgs args)
        {

            ShareOperation shareOperation = args.ShareOperation;
            if (shareOperation.Data.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Bitmap))
            {
                try
                {
                    Stream bitMapStream = (Stream)shareOperation.Data.GetBitmapAsync();
                    ImageFile image = new ImageFile(bitMapStream);
                    image.AddToCache();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (shareOperation.Data.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.StorageItems))
            {
                try
                {
                    IReadOnlyList<IStorageItem> items = await shareOperation.Data.GetStorageItemsAsync();
                    IStorageFile file = (IStorageFile)items[0];
                    string path = file.Path;

                    ImageFile image = new ImageFile(path);
                    image.AddToCache();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            shareOperation.ReportCompleted();
            SingleInstanceManager singleInstanceManager = new SingleInstanceManager();
            singleInstanceManager.Run(Environment.GetCommandLineArgs());
        }

        private static async Task<bool> RegisterPackageWithExternalLocationAsync(string externalLocation, string packagePath)
        {
            bool registration = false;
            try
            {
                var externalUri = new Uri(externalLocation);
                var packageUri = new Uri(packagePath);

                Console.WriteLine("exe Location {0}", externalLocation);
                Console.WriteLine("msix Address {0}", packagePath);

                Console.WriteLine("  exe Uri {0}", externalUri);
                Console.WriteLine("  msix Uri {0}", packageUri);

                var packageManager = new PackageManager();

                //Declare use of an external location
                var options = new AddPackageOptions();
                options.ExternalLocationUri = externalUri;

                Console.WriteLine("Installing package {0}", packagePath);
                Debug.WriteLine("Waiting for package registration to complete...");

                var deploymentOperation = packageManager.AddPackageByUriAsync(packageUri, options);

                await deploymentOperation;

                if (deploymentOperation.Status == Windows.Foundation.AsyncStatus.Error)
                {
                    Windows.Management.Deployment.DeploymentResult deploymentResult = deploymentOperation.GetResults();
                    Debug.WriteLine("Installation Error: {0}", deploymentOperation.ErrorCode);
                    Debug.WriteLine("Detailed Error Text: {0}", deploymentResult.ErrorText);

                }
                else if (deploymentOperation.Status == Windows.Foundation.AsyncStatus.Canceled)
                {
                    Debug.WriteLine("Package Registration Canceled");
                }
                else if (deploymentOperation.Status == Windows.Foundation.AsyncStatus.Completed)
                {
                    registration = true;
                    Debug.WriteLine("Package Registration succeeded!");
                }
                else
                {
                    Debug.WriteLine("Installation status unknown");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddPackageSample failed, error message: {0}", ex.Message);
                Console.WriteLine("Full Stacktrace: {0}", ex.ToString());

                return registration;
            }

            return registration;
        }

        private static async void RemovePackageWithExternalLocationAsync() //example of how to uninstall an identity package
        {
            var packageManager = new PackageManager();

            Debug.WriteLine("Uninstalling package..");
            await packageManager.RemovePackageAsync("PhotoStoreDemo_0.0.0.1_x86__rg009sv5qtcca");
        }
    }
}
