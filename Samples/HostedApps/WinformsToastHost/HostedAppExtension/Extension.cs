using HostedAppInterfaces;

using System.Reflection;

namespace HostedAppExtension
{
    public class Extension : IHostedAppExtension
    {
        public void Run(IHostApp app)
        {
            app.ShowMessage($"Hello from {Assembly.GetExecutingAssembly().GetName().FullName}");
        }
    }
}
