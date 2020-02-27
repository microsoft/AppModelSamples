using System;

namespace HostedAppInterfaces
{
    public interface IHostApp
    {
        void ShowMessage(string message);
    }

    public interface IHostedAppExtension
    {
        void Run(IHostApp app);
    }
}
