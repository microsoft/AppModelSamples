using System.Collections.Generic;
using Windows.System;

namespace TaskMonitor.ViewModels
{
    public class AppProcessesTasks
    {
        public AppInfoDetails AppInfoDetails { get; set; }
        public List<ProcessInfoDetails> PInfos { get; set; }
        public List<AppResourceGroupBackgroundTaskReport> BgTasks { get; set; }
    }
}
