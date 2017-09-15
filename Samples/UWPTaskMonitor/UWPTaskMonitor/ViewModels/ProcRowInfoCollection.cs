using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;

namespace TaskMonitor.ViewModels
{
    public class ProcRowInfoCollection : ObservableCollection<ProcRowInfo>
    {

        #region Fields & properties

        // Note: we don't have a way to differentiate system processes from non-system.
        //private BitmapImage defaultSystemImage;
        private BitmapImage defaultProcessImage;
        private BitmapImage defaultAppImage;
        private Dictionary<string, bool> sortAscending = new Dictionary<string, bool>();

        #endregion


        #region Init

        public ProcRowInfoCollection()
        {
            // We can't get icons for non-UWP processes, so we use default images for Win32 processes
            // vs identified packaged app processes.
            //defaultSystemImage = new BitmapImage(new Uri("ms-appx:/Assets/default-system-icon.png", UriKind.Absolute));
            defaultProcessImage = new BitmapImage(new Uri("ms-appx:/Assets/default-process-icon.png", UriKind.Absolute));
            defaultAppImage = new BitmapImage(new Uri("ms-appx:/Assets/default-app-icon.png", UriKind.Absolute));

            sortAscending.Add("ExecutableFileName", true);
            sortAscending.Add("ProcessId", false);
            sortAscending.Add("KernelTime", false);
            sortAscending.Add("UserTime", false);
            sortAscending.Add("PageFileSizeInBytes", false);
            sortAscending.Add("WorkingSetSizeInBytes", false);
            sortAscending.Add("BytesReadCount", false);
            sortAscending.Add("BytesWrittenCount", false);
            sortAscending.Add("AppType", false);
        }

        #endregion


        #region Update

        public void Update()
        {
            // We're clearing and recreating the process list each time, but the list is typically long (150-250 processes),
            // so we can't use the same add/remove/update technique we use for packaged apps, because it's too slow.
            Clear();

            IReadOnlyList<ProcessDiagnosticInfo> processes = ProcessDiagnosticInfo.GetForProcesses();
            if (processes != null)
            {
                foreach (ProcessDiagnosticInfo process in processes)
                {
                    BitmapImage image = null;
                    if (process.IsPackaged)
                    {
                        image = defaultAppImage;
                    }
                    else
                    {
                        image = defaultProcessImage;
                    }
                    ProcRowInfo processInfo = new ProcRowInfo(process, image);
                    Add(processInfo);
                }
            }
        }

        #endregion


        #region Sort

        public void Sort(string column, bool isUserAction)
        {
            List<ProcRowInfo> sorted = null;
            switch (column)
            {
                case "ExecutableFileName":
                    sorted = this.OrderBy(a => a.ExecutableFileName).ToList();
                    break;
                case "ProcessId":
                    sorted = this.OrderBy(a => a.ProcessId).ToList();
                    break;
                case "KernelTime":
                    sorted = this.OrderBy(a => a.KernelTime).ToList();
                    break;
                case "UserTime":
                    sorted = this.OrderBy(a => a.UserTime).ToList();
                    break;
                case "PageFileSizeInBytes":
                    sorted = this.OrderBy(a => a.PageFileSizeInBytes).ToList();
                    break;
                case "WorkingSetSizeInBytes":
                    sorted = this.OrderBy(a => a.WorkingSetSizeInBytes).ToList();
                    break;
                case "BytesReadCount":
                    sorted = this.OrderBy(a => a.BytesReadCount).ToList();
                    break;
                case "BytesWrittenCount":
                    sorted = this.OrderBy(a => a.BytesWrittenCount).ToList();
                    break;
                case "AppType":
                    sorted = this.OrderBy(a => a.AppType).ToList();
                    break;
            }

            KeyValuePair<string, bool> item = sortAscending.FirstOrDefault(i => i.Key == column);
            bool isAscending = item.Value;
            if (isUserAction)
            {
                // If this is a user clicking the column header, as opposed to a timer-tick update,
                // we toggle the ascending/descending order.
                isAscending = !isAscending;
                sortAscending[column] = isAscending;
            }
            if (isAscending == false)
            {
                sorted.Reverse();
            }

            for (int i = 0; i < sorted.Count(); i++)
            {
                Move(IndexOf(sorted[i]), i);
            }
        }

        #endregion

    }
}
