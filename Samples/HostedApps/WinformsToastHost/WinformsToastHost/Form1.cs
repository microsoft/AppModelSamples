using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace WinformsToastHost
{
    public partial class Form1 : Form
    {
        //private fields holding runtime identity details
        private string _myPackageId = "";
        private string _myPackagePath = "";
        public Form1()
        {
            InitializeComponent();

            //call Windows packaging apis to get identity information
            _myPackageId = Windows.ApplicationModel.Package.Current.Id.FullName;
            _myPackagePath = Windows.ApplicationModel.Package.Current.InstalledPath;

            //display info to the user
            textblockPackageId.Text = _myPackageId;
            textblockPackagePath.Text = _myPackagePath;

            //compare if the executable directory is the same as the package directory, if so, hide the Load extension UX
            if (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location.ToLower()).Contains(_myPackagePath.ToLower()))
            {
                LoadAssembly.Visible = false;
                Message.Visible = false;
            };

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //create toast content, showing packageId
            string toastXml = $@"<toast>
                            <visual>
                                <binding template='ToastGeneric'>
                                    <text>From ToastHost!</text>
                                    <text>My App Identity is: {_myPackageId}</text>
                                </binding>
                            </visual>
                        </toast>";

            XmlDocument toastDoc = new XmlDocument();
            toastDoc.LoadXml(toastXml);

            //create and show the toast
            ToastNotification toast = new ToastNotification(toastDoc);
            ToastNotificationManager.CreateToastNotifier().Show(toast);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Use the extentions's path which we now have access to via the HostedApp model
            //to load the extension dll and call a method using reflection
            AssemblyName an = AssemblyName.GetAssemblyName(Path.Combine(_myPackagePath, "MyExtension.dll"));
            Assembly a = Assembly.Load(an);
            Type myType = a.GetType("MyExtension.Message");
            MethodInfo myMethod = myType.GetMethod("SayHello");
            object obj = Activator.CreateInstance(myType);

            // Execute the extension method
            Message.Text = myMethod.Invoke(obj, null).ToString();
        }
    }
}
