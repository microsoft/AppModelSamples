using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoStoreDemo
{
    public class SingleInstanceManager : WindowsFormsApplicationBase
    {       
        private SingleInstanceApplication _application;
        private System.Collections.ObjectModel.ReadOnlyCollection<string> _commandLine;

        public SingleInstanceManager()
        {
            base.IsSingleInstance = true;
        }

        protected override bool OnStartup(StartupEventArgs e)
        {                       
            // First time _application is launched
            _commandLine = e.CommandLine;
            _application = new SingleInstanceApplication();
            _application.Run();
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs e)
        {
            // Subsequent launches
            base.OnStartupNextInstance(e);
            _commandLine = e.CommandLine;
            _application.Activate();
        }
       
    }

    public class SingleInstanceApplication
    {
        App app; 
        public void Run()
        {
            app = new App();
            app.Run();
        }
        public void Activate()
        {
            app.Activate();
        }
    }
}
