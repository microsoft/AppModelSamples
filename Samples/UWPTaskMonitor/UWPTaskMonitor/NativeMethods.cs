using System;
using System.Runtime.InteropServices;

namespace TaskMonitor
{
    // All of these functions are in the approved list for UWP apps.
    public class NativeMethods
    {

        #region GetLogicalProcessorInformation

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESSORCORE
        {
            public byte Flags;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct NUMANODE
        {
            public uint NodeNumber;
        }

        public enum PROCESSOR_CACHE_TYPE
        {
            CacheUnified,
            CacheInstruction,
            CacheData,
            CacheTrace
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CACHE_DESCRIPTOR
        {
            public byte Level;
            public byte Associativity;
            public ushort LineSize;
            public uint Size;
            public PROCESSOR_CACHE_TYPE Type;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct SYSTEM_LOGICAL_PROCESSOR_INFORMATION_UNION
        {
            [FieldOffset(0)]
            public PROCESSORCORE ProcessorCore;
            [FieldOffset(0)]
            public NUMANODE NumaNode;
            [FieldOffset(0)]
            public CACHE_DESCRIPTOR Cache;
            [FieldOffset(0)]
            private ulong Reserved1;
            [FieldOffset(8)]
            private ulong Reserved2;
        }

        public enum LOGICAL_PROCESSOR_RELATIONSHIP
        {
            RelationProcessorCore,
            RelationNumaNode,
            RelationCache,
            RelationProcessorPackage,
            RelationGroup,
            RelationAll = 0xffff
        }

        public struct SYSTEM_LOGICAL_PROCESSOR_INFORMATION
        {
            public UIntPtr ProcessorMask;
            public LOGICAL_PROCESSOR_RELATIONSHIP Relationship;
            public SYSTEM_LOGICAL_PROCESSOR_INFORMATION_UNION ProcessorInformation;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetLogicalProcessorInformation(IntPtr Buffer, ref uint ReturnLength);

        private const int ERROR_INSUFFICIENT_BUFFER = 122;

        public static SYSTEM_LOGICAL_PROCESSOR_INFORMATION[] GetLogicalProcessorInformation()
        {
            uint ReturnLength = 0;
            GetLogicalProcessorInformation(IntPtr.Zero, ref ReturnLength);
            if (Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
            {
                IntPtr Ptr = Marshal.AllocHGlobal((int)ReturnLength);
                try
                {
                    if (GetLogicalProcessorInformation(Ptr, ref ReturnLength))
                    {
                        int size = Marshal.SizeOf<SYSTEM_LOGICAL_PROCESSOR_INFORMATION>();
                        int len = (int)ReturnLength / size;
                        SYSTEM_LOGICAL_PROCESSOR_INFORMATION[] Buffer = new SYSTEM_LOGICAL_PROCESSOR_INFORMATION[len];
                        IntPtr Item = Ptr;
                        for (int i = 0; i < len; i++)
                        {
                            Buffer[i] = Marshal.PtrToStructure<SYSTEM_LOGICAL_PROCESSOR_INFORMATION>(Item);
                            Item += size;
                        }
                        return Buffer;
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(Ptr);
                }
            }
            return null;
        }

        #endregion


        #region IsProcessorFeaturePresent

        public enum ProcessorFeature : uint
        {
            /// <summary>
            /// On a Pentium, a floating-point precision error can occur in rare circumstances
            /// </summary>
            FloatingPointPrecisionErrata = 0,
            /// <summary>
            /// Floating-point operations are emulated using a software emulator. 
            /// This function returns a nonzero value if floating-point operations are emulated; otherwise, it returns zero.
            /// </summary>
            FloatingPointEmulated = 1,
            /// <summary>
            /// The atomic compare and exchange operation (cmpxchg) is available
            /// </summary>
            CompareExchangeDouble = 2,
            /// <summary>
            /// The MMX instruction set is available
            /// </summary>
            InstructionsMMXAvailable = 3,
            /// <summary>
            /// The SSE instruction set is available
            /// </summary>
            InstructionsXMMIAvailable = 6,
            /// <summary>
            /// The 3D-Now instruction set is available.
            /// </summary>
            Instruction3DNowAvailable = 7,
            /// <summary>
            /// The RDTSC instruction is available
            /// </summary>
            InstructionRDTSCAvailable = 8,
            /// <summary>
            /// The processor is PAE-enabled
            /// </summary>
            PAEEnabled = 9,
            /// <summary>
            /// The SSE2 instruction set is available
            /// </summary>
            InstructionsXMMI64Available = 10,
            /// <summary>
            /// Data execution prevention is enabled. (This feature is not supported until Windows XP SP2 and Windows Server 2003 SP1)
            /// </summary>
            NXEnabled = 12,
            /// <summary>
            /// The SSE3 instruction set is available. (This feature is not supported until Windows Vista)
            /// </summary>
            InstructionsSSE3Available = 13,
            /// <summary>
            /// The atomic compare and exchange 128-bit operation (cmpxchg16b) is available. (This feature is not supported until Windows Vista)
            /// </summary>
            CompareExchange128 = 14,
            /// <summary>
            /// The atomic compare 64 and exchange 128-bit operation (cmp8xchg16) is available (This feature is not supported until Windows Vista.)
            /// </summary>
            Compare64Exchange128 = 15,
            /// <summary>
            /// TBD
            /// </summary>
            ChannelsEnabled = 16,
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsProcessorFeaturePresent(ProcessorFeature processorFeature);

        #endregion


        #region GetSystemInfo

        public enum ProcessorArchitecture : ushort
        {
            Intel = 0, IA64 = 6, AMD64 = 9, Unknown = 0xFFFF
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_INFO
        {
            public ProcessorArchitecture wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        };

        [DllImport("kernel32.dll")]
        public static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        #endregion


        #region MemoryStatus

        [StructLayout(LayoutKind.Sequential)]
        public class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpMemoryStatusEx);

        #endregion


        #region GetSystemPowerStatus

        public enum ACLineStatus : byte
        {
            Offline = 0,
            Online = 1,
            Unknown = 255
        }

        [Flags]
        public enum BatteryFlag : byte
        {
            High = 1,
            Low = 2,
            Critical = 4,
            Charging = 8,
            NoSystemBattery = 128,
            Unknown = 255
        }

        public enum SystemStatusFlag : byte
        {
            Off = 0,
            On = 1
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_POWER_STATUS
        {
            public ACLineStatus ACLineStatus;
            public BatteryFlag BatteryChargeStatus;
            public byte BatteryLifePercent;
            public SystemStatusFlag BatterySaver;
            public int BatteryLifeTime;
            public int BatteryFullLifeTime;
        }

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetSystemPowerStatus(ref SYSTEM_POWER_STATUS lpSystemPowerStatus);

        #endregion


        #region GetDiskFreeSpace

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetDiskFreeSpaceEx(
            string lpDirectoryName, out ulong lpFreeBytesAvailable, 
            out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

        #endregion


        #region GetNetworkParams 

        public const int MAX_HOSTNAME_LEN = 128;
        public const int MAX_SCOPE_ID_LEN = 256;

        public const int ERROR_SUCCESS = 0;
        public const int ERROR_BUFFER_OVERFLOW = 111;

        public const int BROADCAST_NODETYPE = 1;
        public const int PEER_TO_PEER_NODETYPE = 2;
        public const int MIXED_NODETYPE = 4;
        public const int HYBRID_NODETYPE = 8;

        [StructLayout(LayoutKind.Sequential)]
        public struct IP_ADDRESS_STRING
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string Address;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IP_ADDR_STRING
        {
            public IntPtr Next;
            public IP_ADDRESS_STRING IpAddress;
            public IP_ADDRESS_STRING IpMask;
            public Int32 Context;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FIXED_INFO
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_HOSTNAME_LEN + 4)]
            public string HostName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_HOSTNAME_LEN + 4)]
            public string DomainName;
            public IntPtr CurrentDnsServer;
            public IP_ADDR_STRING DnsServerList;
            public uint NodeType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_SCOPE_ID_LEN + 4)]
            public string ScopeId;
            public uint EnableRouting;
            public uint EnableProxy;
            public uint EnableDns;
        }

        [DllImport("iphlpapi.dll", CharSet = CharSet.Ansi)]
        public static extern int GetNetworkParams(IntPtr pFixedInfo, ref UInt32 pBufOutLen);

        #endregion

    }
}
