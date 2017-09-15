namespace TaskMonitor.ViewModels
{
    public class RowAggregateData
    {
        public ulong CommitLimit { get; internal set; }
        public ulong TotalCommit { get; internal set; }
        public ulong PrivateCommit { get; internal set; }
        public int BgTaskCount { get; internal set; }
        public ExecutionStateEx ExecutionState { get; internal set; }
        public EnergyQuotaStateEx EnergyState { get; internal set; }

        public RowAggregateData(
            ulong limit, ulong total, ulong priv, int bg, ExecutionStateEx exState, EnergyQuotaStateEx enState)
        {
            CommitLimit = limit;
            TotalCommit = total;
            PrivateCommit = priv;
            BgTaskCount = bg;
            ExecutionState = exState;
            EnergyState = enState;
        }
    }
}
