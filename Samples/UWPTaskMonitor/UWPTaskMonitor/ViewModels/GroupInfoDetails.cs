using System;
using System.Collections.Generic;
using Windows.System;

namespace TaskMonitor.ViewModels
{
    public class GroupInfoDetails
    {
        public Guid InstanceId { get; internal set; }
        public bool IsShared { get; internal set; }

        public AppMemoryUsageLevel CommitUsageLevel { get; internal set; }
        public ulong CommitUsageLimit { get; internal set; }
        public ulong PrivateCommitUsage { get; internal set; }
        public ulong TotalCommitUsage { get; internal set; }

        public AppResourceGroupExecutionState ExecutionState { get; internal set; }
        public AppResourceGroupEnergyQuotaState EnergyQuotaState { get; internal set; }

        public IList<AppResourceGroupBackgroundTaskReport> BgTasks { get; internal set; }
        public IList<ProcessInfoDetails> Processes { get; internal set; }

        public GroupInfoDetails(
            Guid iid, bool shared,
            AppMemoryUsageLevel cLevel, ulong cLimit, ulong pCommit, ulong tCommit,
            AppResourceGroupExecutionState ex, AppResourceGroupEnergyQuotaState eq,
            IList<AppResourceGroupBackgroundTaskReport> tasks, IList<ProcessInfoDetails> p)
        {
            InstanceId = iid;
            IsShared = shared;
            CommitUsageLevel = cLevel;
            CommitUsageLimit = cLimit;
            PrivateCommitUsage = pCommit;
            TotalCommitUsage = tCommit;
            ExecutionState = ex;
            EnergyQuotaState = eq;
            BgTasks = tasks;
            Processes = p;
        }
    }
}
