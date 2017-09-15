namespace TaskMonitor
{
    public class StaticSystemInfo
    {
        public string MachineName { get; internal set; }
        public string MakeModel { get; internal set; }
        public string FormFactor { get; internal set; }
        public string OSVersion { get; internal set; }
        public string LogicalProcessors { get; internal set; }
        public string Processor { get; internal set; }
        public string PageSize { get; internal set; }
    }
}
