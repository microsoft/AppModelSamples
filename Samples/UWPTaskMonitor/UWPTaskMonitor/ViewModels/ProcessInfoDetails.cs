using System;

namespace TaskMonitor.ViewModels
{
    // The ProcessInfoDetails class is used to collect data for each process
    // for a given resource group, and then displayed in the app Details pivot.
    public class ProcessInfoDetails
    {
        public int ProcessId { get; internal set; }
        public string ExecutableFileName { get; internal set; }
        public DateTimeOffset ProcessStartTime { get; internal set; }

        public TimeSpan KernelTime { get; internal set; }
        public TimeSpan UserTime { get; internal set; }

        public ulong NonPagedPoolSizeInBytes { get; internal set; }
        public ulong PagedPoolSizeInBytes { get; internal set; }
        public ulong PageFaultCount { get; internal set; }
        public ulong PageFileSizeInBytes { get; internal set; }
        public ulong PeakNonPagedPoolSizeInBytes { get; internal set; }
        public ulong PeakPagedPoolSizeInBytes { get; internal set; }
        public ulong PeakPageFileSizeInBytes { get; internal set; }
        public ulong PeakVirtualMemorySizeInBytes { get; internal set; }
        public ulong PeakWorkingSetSizeInBytes { get; internal set; }
        public ulong PrivatePageCount { get; internal set; }
        public ulong VirtualMemorySizeInBytes { get; internal set; }
        public ulong WorkingSetSizeInBytes { get; internal set; }

        public long BytesReadCount { get; internal set; }
        public long BytesWrittenCount { get; internal set; }
        public long OtherBytesCount { get; internal set; }
        public long OtherOperationCount { get; internal set; }
        public long ReadOperationCount { get; internal set; }
        public long WriteOperationCount { get; internal set; }

        public ProcessInfoDetails(
            uint pid, string name, DateTimeOffset start,
            TimeSpan kernel, TimeSpan user,
            ulong npp, ulong pp, ulong pFault, ulong pFile, ulong pNpp, ulong pPP, ulong ppFile, ulong pVirt, ulong pWSet, ulong ppc, ulong vm, ulong ws,
            long br, long bw, long ob, long oo, long ro, long wo)
        {
            ProcessId = (int)pid;
            ExecutableFileName = name;
            ProcessStartTime = start;

            KernelTime = kernel;
            UserTime = user;

            NonPagedPoolSizeInBytes = npp;
            PagedPoolSizeInBytes = pp;
            PageFaultCount = pFault;
            PageFileSizeInBytes = pFile;
            PeakNonPagedPoolSizeInBytes = pNpp;
            PeakPagedPoolSizeInBytes = pp;
            PeakPageFileSizeInBytes = ppFile;
            PeakVirtualMemorySizeInBytes = pVirt;
            PeakWorkingSetSizeInBytes = pWSet;
            PrivatePageCount = ppc;
            VirtualMemorySizeInBytes = vm;
            WorkingSetSizeInBytes = ws;

            BytesReadCount = br;
            BytesWrittenCount = bw;
            OtherBytesCount = ob;
            OtherOperationCount = oo;
            ReadOperationCount = ro;
            WriteOperationCount = wo;
        }
    }
}
