using System;
using Windows.System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;

namespace TaskMonitor.ViewModels
{
    // ProcRowInfo represents a row of data for a single app, essentially flattening
    // the ProcessDiagnosticInfo type, for easier databinding.
    // We only expose the properties this app cares about.
    public class ProcRowInfo 
    {
        private ProcessDiagnosticInfo pdi;
        public ProcRowInfo(ProcessDiagnosticInfo p, BitmapImage bmp)
        {
            pdi = p;
            cpuReport = pdi.CpuUsage.GetReport();
            diskReport = pdi.DiskUsage.GetReport();
            memoryReport = pdi.MemoryUsage.GetReport();
            Logo = bmp;
            AppType = p.IsPackaged ? "Packaged" : "Win32";
        }

        public string ExecutableFileName { get { return pdi.ExecutableFileName; } }
        public uint ProcessId { get { return pdi.ProcessId; } }
        public DateTimeOffset ProcessStartTime { get { return pdi.ProcessStartTime; } }

        // NOTE accessing ProcessDiagnosticInfo.Parent throws.
        //public uint ParentExecutableProcessId { get { return 0; } } // pdi.Parent.ProcessId; } }
        //public ProcessDiagnosticInfo Parent { get { return null; } } // pdi.Parent; } }

        // ProcessCpuUsageReport
        private ProcessCpuUsageReport cpuReport;
        public TimeSpan KernelTime { get { return cpuReport != null ? cpuReport.KernelTime : TimeSpan.MinValue; } }
        public TimeSpan UserTime { get { return cpuReport != null ? cpuReport.UserTime : TimeSpan.MinValue; } }
        public TimeSpan CpuTime { get { return cpuReport != null ? cpuReport.KernelTime + cpuReport.UserTime : TimeSpan.MinValue; } }

        // ProcessMemoryUsageReport: we're only reporting a few of these properties, so we don't need to declare them all.
        private ProcessMemoryUsageReport memoryReport;
        //public ulong NonPagedPoolSizeInBytes { get { return memoryReport.NonPagedPoolSizeInBytes; } }
        //public ulong PagedPoolSizeInBytes { get { return memoryReport.PagedPoolSizeInBytes; } }
        //public uint PageFaultCount { get { return memoryReport.PageFaultCount; } }
        public ulong PageFileSizeInBytes { get { return memoryReport != null ? memoryReport.PageFileSizeInBytes : ulong.MinValue; } }
        //public ulong PeakNonPagedPoolSizeInBytes { get { return memoryReport.PeakNonPagedPoolSizeInBytes; } }
        //public ulong PeakPagedPoolSizeInBytes { get { return memoryReport.PeakPagedPoolSizeInBytes; } }
        //public ulong PeakPageFileSizeInBytes { get { return memoryReport.PeakPageFileSizeInBytes; } }
        //public ulong PeakVirtualMemorySizeInBytes { get { return memoryReport.PeakVirtualMemorySizeInBytes; } }
        //public ulong PeakWorkingSetSizeInBytes { get { return memoryReport.PeakWorkingSetSizeInBytes; } }
        //public ulong PrivatePageCount { get { return memoryReport.PrivatePageCount; } }
        //public ulong VirtualMemorySizeInBytes { get { return memoryReport.VirtualMemorySizeInBytes; } }
        public ulong WorkingSetSizeInBytes { get { return memoryReport != null ? memoryReport.WorkingSetSizeInBytes : ulong.MinValue; } }

        // ProcessDiskUsageReport
        private ProcessDiskUsageReport diskReport;

        public ulong BytesReadCount { get { return diskReport != null ? (ulong)diskReport.BytesReadCount : ulong.MinValue; } }
        public ulong BytesWrittenCount { get { return diskReport != null ? (ulong)diskReport.BytesWrittenCount : ulong.MinValue; } }
        //public long OtherBytesCount { get { return diskReport.OtherBytesCount; } }
        //public long OtherOperationCount { get { return diskReport.OtherOperationCount; } }
        //public long ReadOperationCount { get { return diskReport.ReadOperationCount; } }
        //public long WriteOperationCount { get { return diskReport.WriteOperationCount; } }
        public ulong DiskBytes { get { return diskReport != null ? (ulong)(diskReport.BytesReadCount + diskReport.BytesWrittenCount) : ulong.MinValue; } }

        public BitmapImage Logo { get; set; }
        public string AppType { get; set; }
        public string Session { get; set; }
    }
}
