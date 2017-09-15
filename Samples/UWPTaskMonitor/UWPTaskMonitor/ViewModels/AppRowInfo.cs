using System.ComponentModel;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;

namespace TaskMonitor.ViewModels
{
    // For roll-up purposes, we extend the ExecutionState/EnergyQuotaState enums with Multiple.
    // For UI, we add NotApplicable (which gets represented as an empty string).
    public enum ExecutionStateEx
    {
        Unknown, Running, Suspending, Suspended, NotRunning,
        Multiple, NotApplicable
    }

    public enum EnergyQuotaStateEx
    {
        Unknown, Over, Under,
        Multiple, NotApplicable
    }

    // AppRowInfo represents a row of data for a single app, essentially flattening
    // the AppDiagnosticInfo type, for easier databinding.
    public class AppRowInfo : INotifyPropertyChanged
    {
        public AppDiagnosticInfo Adi { get; internal set; }
        public string Id { get; internal set; }
        public string ParentId { get; internal set; }
        public BitmapImage Logo { get; internal set; }
        public string Name { get; internal set; }
        public int ProcessId { get; internal set; }

        private ulong commitLimit;
        public ulong CommitLimit
        {
            get { return commitLimit; }
            set
            {
                if (commitLimit != value)
                {
                    commitLimit = value;
                    NotifyPropertyChanged("CommitLimit");
                }
            }
        }

        private ulong totalCommit;
        public ulong TotalCommit
        {
            get { return totalCommit; }
            set
            {
                if (totalCommit != value)
                {
                    totalCommit = value;
                    NotifyPropertyChanged("TotalCommit");
                }
            }
        }

        private ulong privateCommit;
        public ulong PrivateCommit
        {
            get { return privateCommit; }
            set
            {
                if (privateCommit != value)
                {
                    privateCommit = value;
                    NotifyPropertyChanged("PrivateCommit");
                }
            }
        }

        private ExecutionStateEx executionState;
        public ExecutionStateEx ExecutionState
        {
            get { return executionState; }
            set
            {
                if (executionState != value)
                {
                    executionState = value;
                    NotifyPropertyChanged("ExecutionState");
                }
            }
        }

        private EnergyQuotaStateEx energyQuotaState;
        public EnergyQuotaStateEx EnergyQuotaState
        {
            get { return energyQuotaState; }
            set
            {
                if (energyQuotaState != value)
                {
                    energyQuotaState = value;
                    NotifyPropertyChanged("EnergyQuotaState");
                }
            }
        }

        private int backgroundTaskCount;
        public int BackgroundTaskCount
        {
            get { return backgroundTaskCount; }
            set
            {
                if (backgroundTaskCount != value)
                {
                    backgroundTaskCount = value;
                    NotifyPropertyChanged("BackgroundTaskCount");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AppRowInfo(
            AppDiagnosticInfo a,
            string id,
            BitmapImage logo, string name,
            ulong limit, ulong totCommit, ulong prvCommit,
            ExecutionStateEx exState, EnergyQuotaStateEx eqState, int taskCount)
        {
            Adi = a;
            Id = id;
            Logo = logo;
            Name = name;

            commitLimit = limit;
            totalCommit = totCommit;
            privateCommit = prvCommit;
            executionState = exState;
            energyQuotaState = eqState;
            backgroundTaskCount = taskCount;
        }

        public void Update(
            ulong limit, ulong total, ulong commit, 
            ExecutionStateEx exState, EnergyQuotaStateEx enState, int bgCount)
        {
            CommitLimit = limit;
            TotalCommit = total;
            PrivateCommit = commit;
            ExecutionState = exState;
            EnergyQuotaState = enState;
            BackgroundTaskCount = bgCount;
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
