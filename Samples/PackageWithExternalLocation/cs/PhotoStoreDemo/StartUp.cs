using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using Windows.Management.Deployment;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Data;
using System.IO;
using System.Windows;

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
                //TODO - update the value of externalLocation to match the output location of your VS Build binaries and the value of 
                //packagePath to match the path to your signed identity package (.msix). 
                //Note that these values cannot be relative paths and must be complete paths
                string externalLocation = @"";
                string packagePath = @"";

                //Attempt registration
                if (RegisterPackageWithExternalLocation(externalLocation, packagePath))
                {
                    //Registration succeded, restart the app to run with identity
                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location, arguments: cmdArgs?.ToString());

                }
                else //Registration failed, run without identity
                {
                    Debug.WriteLine("Package Registation failed, running WITHOUT Identity");
                    SingleInstanceManager wrapper = new SingleInstanceManager();
                    wrapper.Run(cmdArgs);
                }

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

        private static bool RegisterPackageWithExternalLocation(string externalLocation, string packagePath)
        {
            bool registration = false;
            try
            {
                Uri externalUri = new Uri(externalLocation);
                Uri packageUri = new Uri(packagePath);

                Console.WriteLine("exe Location {0}", externalLocation);
                Console.WriteLine("msix Address {0}", packagePath);

                Console.WriteLine("  exe Uri {0}", externalUri);
                Console.WriteLine("  msix Uri {0}", packageUri);

                PackageManager packageManager = new PackageManager();

                //Declare use of an external location
                var options = new AddPackageOptions();
                options.ExternalLocationUri = externalUri;

                Windows.Foundation.IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> deploymentOperation = packageManager.AddPackageByUriAsync(packageUri, options);

                ManualResetEvent opCompletedEvent = new ManualResetEvent(false); // this event will be signaled when the deployment operation has completed.

                deploymentOperation.Completed = (depProgress, status) => { opCompletedEvent.Set(); };

                Console.WriteLine("Installing package {0}", packagePath);

                Debug.WriteLine("Waiting for package registration to complete...");

                opCompletedEvent.WaitOne();

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

        private static void RemovePackageWithExternalLocation() //example of how to uninstall an identity package
        {
            PackageManager packageManager = new PackageManager();
            Windows.Foundation.IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> deploymentOperation = packageManager.RemovePackageAsync("PhotoStoreDemo_0.0.0.1_x86__rg009sv5qtcca");
            ManualResetEvent opCompletedEvent = new ManualResetEvent(false); // this event will be signaled when the deployment operation has completed.

            deploymentOperation.Completed = (depProgress, status) => { opCompletedEvent.Set(); };

            Debug.WriteLine("Uninstalling package..");
            opCompletedEvent.WaitOne();
        }

    }
}
