namespace TaskMonitor
{
    public class DynamicSystemInfo
    {
        public string PhysicalMemory { get; internal set; }
        public string PhysicalPlusPagefile { get; internal set; }
        public string VirtualMemory { get; internal set; }
        public string PagefileOnDisk { get; internal set; }
        public string MemoryLoad { get; internal set; }

        public string ACLineStatus { get; internal set; }
        public string BatteryChargeStatus { get; internal set; }
        public string BatteryLife { get; internal set; }
        public string BatterySaver { get; internal set; }

        public string ChargeRate { get; internal set; }
        public string Capacity { get; internal set; }

        public string TotalDiskSize { get; internal set; }
        public string DiskFreeSpace { get; internal set; }

        public string DomainName { get; internal set; }
        public string NodeType { get; internal set; }

        public string ConnectedProfile { get; internal set; }
        public string IanaInterfaceType { get; internal set; }
        public string InboundSpeed { get; internal set; }
        public string OutboundSpeed { get; internal set; }
        public string HostAddress { get; internal set; }
        public string AddressType { get; internal set; }
    }
}
