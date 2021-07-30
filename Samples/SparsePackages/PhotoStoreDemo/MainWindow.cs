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

// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.Reflection;
using Windows.Management.Deployment;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Principal;

namespace PhotoStoreDemo
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Stack _undoStack;
        private RubberbandAdorner _cropSelector;
        public PhotoList Photos;
        public PrintList ShoppingCart;

        public MainWindow()
        {
            _undoStack = new Stack();
            InitializeComponent();
            buttonAddPhoto.ToolTip = PhotosFolder.Current;
        }



        private void WindowLoaded(object sender, EventArgs e)
        {

            if (ExecutionMode.IsRunningWithIdentity())
            {
                this.Title = "PhotoStoreDemo --- (Running with Identity)";
                this.TitleSpan.Foreground = Brushes.Blue;
            }
            else
            {
                this.Title = "Desktop App";
                this.TitleSpan.Foreground = Brushes.Navy;
            }

            var layer = AdornerLayer.GetAdornerLayer(CurrentPhoto);
            _cropSelector = new RubberbandAdorner(CurrentPhoto) { Window = this };
            layer.Add(_cropSelector);
#if VISUALCHILD
            CropSelector.Rubberband.Visibility = Visibility.Hidden;
#endif
#if NoVISUALCHILD
            CropSelector.ShowRect = false;
#endif

            Photos = (PhotoList)(this.Resources["Photos"] as ObjectDataProvider)?.Data;
            Photos.Init(PhotosFolder.Current);
            ShoppingCart = (PrintList)(this.Resources["ShoppingCart"] as ObjectDataProvider)?.Data;

            // listen for files being created via Share UX
            FileSystemWatcher watcher = new FileSystemWatcher(PhotosFolder.Current);
            watcher.EnableRaisingEvents = true;
            watcher.Created += Watcher_Created;
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            // new file got created, adding it to the list
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                if (File.Exists(e.FullPath))
                {
                    ImageFile item = new ImageFile(e.FullPath);
                    Photos.Insert(0, item);
                    PhotoListBox.SelectedIndex = 0;
                    CurrentPhoto.Source = (BitmapSource)item.Image;
                }
            }));
        }

        private void PhotoListSelection(object sender, RoutedEventArgs e)
        {
            var path = ((sender as ListBox)?.SelectedItem.ToString());
            BitmapSource img = BitmapFrame.Create(new Uri(path));
            CurrentPhoto.Source = img;
            ClearUndoStack();
            if (_cropSelector != null)
            {
#if VISUALCHILD
                if (Visibility.Visible == CropSelector.Rubberband.Visibility)
                    CropSelector.Rubberband.Visibility = Visibility.Hidden;
#endif
#if NoVISUALCHILD
                if (CropSelector.ShowRect)
                    CropSelector.ShowRect=false;
#endif
            }
            CropButton.IsEnabled = false;
        }

        private void AddToShoppingCart(object sender, RoutedEventArgs e)
        {
            if (PrintTypeComboBox.SelectedItem != null)
            {
                PrintBase item;
                switch (PrintTypeComboBox.SelectedIndex)
                {
                    case 0:
                        item = new Print(CurrentPhoto.Source as BitmapSource);
                        break;
                    case 1:
                        item = new GreetingCard(CurrentPhoto.Source as BitmapSource);
                        break;
                    case 2:
                        item = new Shirt(CurrentPhoto.Source as BitmapSource);
                        break;
                    default:
                        return;
                }
                ShoppingCart.Add(item);
                ShoppingCartListBox.ScrollIntoView(item);
                ShoppingCartListBox.SelectedItem = item;
                if (false == UploadButton.IsEnabled)
                    UploadButton.IsEnabled = true;
                if (false == RemoveButton.IsEnabled)
                    RemoveButton.IsEnabled = true;
            }
        }

        private void RemoveShoppingCartItem(object sender, RoutedEventArgs e)
        {
            if (null != ShoppingCartListBox.SelectedItem)
            {
                var item = ShoppingCartListBox.SelectedItem as PrintBase;
                ShoppingCart.Remove(item);
                ShoppingCartListBox.SelectedIndex = ShoppingCart.Count - 1;
            }
            if (0 == ShoppingCart.Count)
            {
                RemoveButton.IsEnabled = false;
                UploadButton.IsEnabled = false;
            }
        }

        private void Upload(object sender, RoutedEventArgs e)
        {
            if (ShoppingCart.Count > 0)
            {
                var scaleDuration = new TimeSpan(0, 0, 0, 0, ShoppingCart.Count * 200);
                var progressAnimation = new DoubleAnimation(0, 100, scaleDuration, FillBehavior.Stop);
                UploadProgressBar.BeginAnimation(RangeBase.ValueProperty, progressAnimation);
                ShoppingCart.Clear();
                UploadButton.IsEnabled = false;
                if (RemoveButton.IsEnabled)
                    RemoveButton.IsEnabled = false;
            }
        }

        private void Rotate(object sender, RoutedEventArgs e)
        {
            if (CurrentPhoto.Source != null)
            {
                var img = (BitmapSource)(CurrentPhoto.Source);
                _undoStack.Push(img);
                var cache = new CachedBitmap(img, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                CurrentPhoto.Source = new TransformedBitmap(cache, new RotateTransform(90.0));
                if (false == UndoButton.IsEnabled)
                    UndoButton.IsEnabled = true;
                if (_cropSelector != null)
                {
#if VISUALCHILD
                    if (Visibility.Visible == CropSelector.Rubberband.Visibility)
                        CropSelector.Rubberband.Visibility = Visibility.Hidden;
#endif
#if NoVISUALCHILD
                if (CropSelector.ShowRect)
                    CropSelector.ShowRect=false;
#endif
                }
                CropButton.IsEnabled = false;
            }
        }

        private void BlackAndWhite(object sender, RoutedEventArgs e)
        {
            if (CurrentPhoto.Source != null)
            {
                var img = (BitmapSource)(CurrentPhoto.Source);
                _undoStack.Push(img);
                CurrentPhoto.Source = new FormatConvertedBitmap(img, PixelFormats.Gray8, BitmapPalettes.Gray256, 1.0);
                if (false == UndoButton.IsEnabled)
                    UndoButton.IsEnabled = true;
                if (_cropSelector != null)
                {
#if VISUALCHILD
                    if (Visibility.Visible == CropSelector.Rubberband.Visibility)
                        CropSelector.Rubberband.Visibility = Visibility.Hidden;
#endif
#if NoVISUALCHILD
                    if (CropSelector.ShowRect)
                        CropSelector.ShowRect = false;
#endif
                }
                CropButton.IsEnabled = false;
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var anchor = e.GetPosition(CurrentPhoto);
            _cropSelector.CaptureMouse();
            _cropSelector.StartSelection(anchor);
            CropButton.IsEnabled = true;
        }

        private void Crop(object sender, RoutedEventArgs e)
        {
            if (CurrentPhoto.Source != null)
            {
                var img = (BitmapSource)(CurrentPhoto.Source);
                _undoStack.Push(img);
                var rect = new Int32Rect
                {
                    X = (int)(_cropSelector.SelectRect.X * img.PixelWidth / CurrentPhoto.ActualWidth),
                    Y = (int)(_cropSelector.SelectRect.Y * img.PixelHeight / CurrentPhoto.ActualHeight),
                    Width = (int)(_cropSelector.SelectRect.Width * img.PixelWidth / CurrentPhoto.ActualWidth),
                    Height = (int)(_cropSelector.SelectRect.Height * img.PixelHeight / CurrentPhoto.ActualHeight)
                };
                CurrentPhoto.Source = new CroppedBitmap(img, rect);
#if VISUALCHILD
                if (Visibility.Visible == CropSelector.Rubberband.Visibility)
                    CropSelector.Rubberband.Visibility = Visibility.Hidden;
#endif
#if NoVISUALCHILD
                if (CropSelector.ShowRect)
                    CropSelector.ShowRect = false;
#endif
                CropButton.IsEnabled = false;
                if (false == UndoButton.IsEnabled)
                    UndoButton.IsEnabled = true;
            }
        }

        private void Undo(object sender, RoutedEventArgs e)
        {
            if (_undoStack.Count > 0)
                CurrentPhoto.Source = (BitmapSource)_undoStack.Pop();
            if (0 == _undoStack.Count)
                UndoButton.IsEnabled = false;
#if VISUALCHILD
                if (Visibility.Visible == CropSelector.Rubberband.Visibility)
                    CropSelector.Rubberband.Visibility = Visibility.Hidden;
#endif
#if NoVISUALCHILD
            if (CropSelector.ShowRect)
                CropSelector.ShowRect = false;
#endif
        }

        private void ClearUndoStack()
        {
            _undoStack.Clear();
            UndoButton.IsEnabled = false;
        }

        private void AddPhoto(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog() { Filter = "JPG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|GIF Files (*.gif)|*.gif" };
            var result = ofd.ShowDialog();
            if (result == false) return;
            ImageFile item = new ImageFile(ofd.FileName);
            item.AddToCache();
        }

        private void Add_Via_Toast_Click(object sender, RoutedEventArgs e)
        {
            var toastManager = ToastNotificationManager.GetDefault();
            var toastNotifier = toastManager.CreateToastNotifier();
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(@"
                <toast>
                    <visual>
                      <binding template='ToastGeneric'>
                        <text>Send an image path to the app</text>  
                      </binding>
                    </visual>
                    <actions>
                        <input id='textBox' type='text' placeHolderContent='Type a reply'/>
                        <action
                            content='Send'
                            arguments='action=send'
                            activationType='foreground'/>
                        <action
                            content='Cancel'
                            arguments='dismiss'
                            activationType='system'/>
                    </actions>
                </toast>");
            ToastNotification notification = new ToastNotification(xml);
            toastNotifier.Show(notification);
        }

        private async void LooseFileRegistration_Click(object sender, RoutedEventArgs e)
        {
            //register loose file, aka "devmode"
            var success = await registerSparsePackage(true, true);
            //if success, restart app
            DisplayResult(success);
        }

        private async void UnsignedMSIXRegistration_Click(object sender, RoutedEventArgs e)
        {
            //check elevation level
            if (!IsElevated)
            {
                MessageBox.Show("Process is not running elevated, try RunAs Admin");
                return;
            }
            //register msix, allowunsigned=true
            var success = await registerSparsePackage(false, true);
            DisplayResult(success);

        }


        private async void SignedMSIXRegistration_Click(object sender, RoutedEventArgs e)
        {
            //register msix
            var success = await registerSparsePackage(false, false);
            //if success, restart app
            DisplayResult(success);

        }

        private static async Task<bool> registerSparsePackage(bool registerLooseFile, bool AllowUnsigned = false)
        {
            bool registration = false;

            //get the manifest or msix package 
            //this code assumes it is in the same folder as the EXE
            string fileName = Process.GetCurrentProcess().MainModule.FileName;
            string externalLocation = Path.GetDirectoryName(fileName);

            //for loose file (aka DevMode) registration, pass in just the path to the folder that contains appxmanifest.xml
            //for msix package registration, use the msix filename
            string registerFileName = registerLooseFile ? "appxmanifest.xml" : Path.GetFileNameWithoutExtension(fileName) + ".msix";
            //
            string sparsePkgPath = Path.Combine(externalLocation, registerFileName);
            //string sparsePkgPath = @"E:\repos\AppModelSamples\Samples\SparsePackages\PhotoStoreDemoPkg";

            try
            {
                Uri externalUri = new Uri(externalLocation);
                Uri packageUri = new Uri(sparsePkgPath);

                Debug.WriteLine("exe Location {0}", externalLocation);
                Debug.WriteLine("msix Address {0}", sparsePkgPath);

                Debug.WriteLine("  exe Uri {0}", externalUri);
                Debug.WriteLine("  msix Uri {0}", packageUri);

                PackageManager packageManager = new PackageManager();

                //Declare use of an external location
                //var options = new AddPackageOptions();
                var options = new RegisterPackageOptions();
                options.ExternalLocationUri = externalUri;
                if (AllowUnsigned)
                {
                    //options.AllowUnsigned = AllowUnsigned;
                    options.DeveloperMode = true;
                        
                }

                //Windows.Foundation.IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> deploymentOperation = packageManager.AddPackageByUriAsync(packageUri, options);
                //var deploymentOperation = await packageManager.AddPackageByUriAsync(packageUri, options);
                var deploymentOperation = await packageManager.RegisterPackageByUriAsync(packageUri, options);
                registration = deploymentOperation.IsRegistered;

                #region oldcode
                //ManualResetEvent opCompletedEvent = new ManualResetEvent(false); // this event will be signaled when the deployment operation has completed.

                //deploymentOperation.Completed = (depProgress, status) => { opCompletedEvent.Set(); };

                //Console.WriteLine("Installing package {0}", sparsePkgPath);

                //Debug.WriteLine("Waiting for package registration to complete...");

                //opCompletedEvent.WaitOne();

                //if (deploymentOperation.Status == Windows.Foundation.AsyncStatus.Error)
                //{
                //    Windows.Management.Deployment.DeploymentResult deploymentResult = deploymentOperation.GetResults();
                //    Debug.WriteLine("Installation Error: {0}", deploymentOperation.ErrorCode);
                //    Debug.WriteLine("Detailed Error Text: {0}", deploymentResult.ErrorText);

                //}
                //else if (deploymentOperation.Status == Windows.Foundation.AsyncStatus.Canceled)
                //{
                //    Debug.WriteLine("Package Registration Canceled");
                //}
                //else if (deploymentOperation.Status == Windows.Foundation.AsyncStatus.Completed)
                //{
                //    registration = true;
                //    Debug.WriteLine("Package Registration succeeded!");
                //}
                //else
                //{
                //    Debug.WriteLine("Installation status unknown");
                //}
                #endregion
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AddPackageSample failed, error message: {0}", ex.Message);
                Debug.WriteLine("Full Stacktrace: {0}", ex.ToString());

                return registration;
            }

            return registration;
        }

        private static void DisplayResult(bool success)
        {
            if (success)
            {
                //if success, restart app
                MessageBox.Show("Registration Success! Please restart app");
            }
            else
            {
                MessageBox.Show("Registration Failed");
            }
        }

        public static bool IsElevated
        {
            get
            {
                return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}