using HostedAppInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace WinformsToastHost
{
    // The form implements the IHostApp interface, which simply allows the hosted app
    // to display messages.
    public partial class Form1 : Form, IHostApp
    {
        public Form1()
        {
            InitializeComponent();

            // Debug info - show the ID and path
            txtPackageId.Text = Program.PackageId;
            txtPackagePath.Text = Program.PackageDirectory;

            // If the app is NOT running as a host, there are no hosted app assemblies available and the
            // app should just do it's normal thing. In this case, we hide the UX to load the extension.
            if (!Program.IsRunningAsHost)
            {
                btnRunHostedApp.Visible = false;
            }

            // If the app is not running with a package identity, it can't show a toast.
            if (!Program.HasPackageIdentity)
            {
                btnShowToast.Visible = false;
            }
        }

        private void btnShowToast_Click(object sender, EventArgs e)
        {
            // Create toast content, showing the PackageId
            string toastXml = $@"   <toast>
                                        <visual>
                                            <binding template='ToastGeneric'>
                                                <text>From ToastHost!</text>
                                                <text>My App Identity is: {Program.PackageId}</text>
                                            </binding>
                                        </visual>
                                    </toast>";

            XmlDocument toastDoc = new XmlDocument();
            toastDoc.LoadXml(toastXml);

            // Create and show the toast
            ToastNotification toast = new ToastNotification(toastDoc);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private void btnRunHostedApp_Click(object sender, EventArgs e)
        {
            // The contract this host has is that its hosted apps must expose a binary named 
            // "HostedAppExtension\HostedAppExtension.dll" and it must contain a type 
            // named "HostedAppExtension.Extension" that implements the "IHostedAppExtension" 
            // interface.
            //
            // A more complex solution would have a richer set of interfaces and allow
            // the hosted app to pass parameters such as the name of the DLL, etc.
            Type hostedType;
            IHostedAppExtension extension;
            try
            {
                // Use the hosted app's path to load the DLL and call a method using reflection.
                Assembly hostedApp = Assembly.LoadFrom(Path.Combine(Program.PackageDirectory, "HostedAppExtension\\HostedAppExtension.dll"));
                hostedType = hostedApp.GetType("HostedAppExtension.Extension");
                extension = (IHostedAppExtension)Activator.CreateInstance(hostedType);

                // Call the "Run" method, passing ourselves (IHostApp) as the parameter
                hostedType.GetMethod("Run").Invoke(extension, new[] { this });
            }
            catch (BadImageFormatException)
            {
                MessageBox.Show($"Can't load hosted assembly; was it built for the right CPU architecture ({IntPtr.Size * 8}-bit)?", "Error loading hosted app", MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Extension was loaded, but Run failed with error {ex.Message}", "Error running hosted app", MessageBoxButtons.OK);
            }
        }

        public void ShowMessage(string message)
        {
            MessageBox.Show(message, "Message from hosted app", MessageBoxButtons.OK);
        }
    }
}
