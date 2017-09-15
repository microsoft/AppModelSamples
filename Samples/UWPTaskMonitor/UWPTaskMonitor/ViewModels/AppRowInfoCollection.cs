using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.System;
using Windows.System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;

namespace TaskMonitor.ViewModels
{
    public class AppRowInfoCollection : ObservableCollection<AppRowInfo>
    {

        #region Init

        public static readonly object lockObject = new object();
        private Dictionary<string, bool> sortAscending = new Dictionary<string, bool>();

        public AppRowInfoCollection()
        {
            // Populate the list of columns on which we might be asked to sort.
            sortAscending.Add("Name", true);
            sortAscending.Add("CommitLimit", false);
            sortAscending.Add("TotalCommit", false);
            sortAscending.Add("PrivateCommit", false);
            sortAscending.Add("ExecutionState", false);
            sortAscending.Add("EnergyQuotaState", false);
            sortAscending.Add("BackgroundTaskCount", false);
        }

        async public Task<DiagnosticAccessStatus> GetAccessStatus()
        {
            // Request user consent to use the diagnostic APIs.
            DiagnosticAccessStatus accessStatus = await AppDiagnosticInfo.RequestAccessAsync();
            return accessStatus;
        }
    
        #endregion


        #region Update

        public bool Update()
        {
            bool result = false;

            // If we don't lock the collection, there's a race where databinding could end up with 
            // additional spurious instances, especially if the polling interval is only 1-2 sec.
            lock (lockObject)
            {
                // Get a list of running apps (filtered by permission level).
                IAsyncOperation<IList<AppDiagnosticInfo>> infoOperation = AppDiagnosticInfo.RequestInfoAsync();
                Task<IList<AppDiagnosticInfo>> infoTask = infoOperation.AsTask();
                infoTask.Wait();
                IList<AppDiagnosticInfo> runningApps = infoTask.Result;

                // We don't recreate the list each time: instead, we remove dead rows and update existing ones,
                // only adding new rows for new apps that appear. This is slow, but typically we'd only have a small
                // number of UWP apps running (say 5-20), so it's not noticeable.
                if (runningApps != null)
                {
                    // First, remove from the list any apps that are no longer in memory.
                    RemoveDeadRows(runningApps);

                    // Find key data from the bottom-up (process and group level) for roll-up purposes.
                    foreach (AppDiagnosticInfo app in runningApps)
                    {
                        ulong appCommitLimit = 0;
                        ulong appTotalCommit = 0;
                        ulong appPrivateCommit = 0;
                        ExecutionStateEx appExecutionState = ExecutionStateEx.Unknown;
                        EnergyQuotaStateEx appEnergyState = EnergyQuotaStateEx.Unknown;
                        int appBgTaskCount = 0;

                        // Get the roll-up data from group and process level.
                        IList<AppResourceGroupInfo> groups = app.GetResourceGroups();
                        if (groups != null)
                        {
                            foreach (AppResourceGroupInfo group in groups)
                            {
                                Debug.WriteLine("app: {0}, group Id: {1}", app.AppInfo.DisplayInfo.DisplayName, group.InstanceId);

                                // Get the total private commit usage of all processes for this app.
                                ulong totalProcessPrivateCommit = 0;
                                IList<ProcessDiagnosticInfo> processes = group.GetProcessDiagnosticInfos();
                                if (processes != null)
                                {
                                    foreach (ProcessDiagnosticInfo process in processes)
                                    {
                                        totalProcessPrivateCommit += GetProcessPrivateCommit(process);
                                    }
                                }

                                // Accumulate the aggregated totals for all resource groups for this app.
                                // We pass down the private commit for all processes for this group for comparison only.
                                RowAggregateData groupData = GetGroupAggregateData(
                                    app.AppInfo.DisplayInfo.DisplayName, group, totalProcessPrivateCommit);

                                appCommitLimit += groupData.CommitLimit;
                                appTotalCommit += groupData.TotalCommit;
                                appPrivateCommit += groupData.PrivateCommit;
                                appBgTaskCount += groupData.BgTaskCount;

                                if (appExecutionState != groupData.ExecutionState)
                                {
                                    if (appExecutionState == ExecutionStateEx.Unknown)
                                    {
                                        appExecutionState = groupData.ExecutionState;
                                    }
                                    else
                                    {
                                        appExecutionState = ExecutionStateEx.Multiple;
                                    }
                                }
                                if (appEnergyState != groupData.EnergyState)
                                {
                                    if (appEnergyState == EnergyQuotaStateEx.Unknown)
                                    {
                                        appEnergyState = groupData.EnergyState;
                                    }
                                    else
                                    {
                                        appEnergyState = EnergyQuotaStateEx.Multiple;
                                    }
                                }
                            }
                        }

                        RowAggregateData appData = new RowAggregateData(
                            appCommitLimit, appTotalCommit, appPrivateCommit, appBgTaskCount, appExecutionState, appEnergyState);

                        // Now add or update the rows.
                        bool isAppAdded = AddOrUpdateApp(app, appData, appExecutionState);
                        if (isAppAdded)
                        {
                            // If any row update resulted in adding an app, we set the return value to true.
                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        #endregion


        #region RemoveDeadRows

        // Given an updated list of running apps, go through our cached list,
        // and remove any that are no longer alive.
        private void RemoveDeadRows(IList<AppDiagnosticInfo> runningApps)
        {
            for (int i = 0; i < Count; i++)
            {
                bool isFound = false;
                AppRowInfo appRow = this[i];
                foreach (AppDiagnosticInfo app in runningApps)
                {
                    if (app.AppInfo.AppUserModelId == appRow.Id)
                    {
                        isFound = true;
                    }
                }
                if (!isFound)
                {
                    Remove(appRow);
                }
            }
        }

        #endregion


        #region GetRollupData

        private ulong GetProcessPrivateCommit(ProcessDiagnosticInfo process)
        {
            ulong privateCommit = 0;
            if (process.MemoryUsage != null)
            {
                ProcessMemoryUsageReport pmReport = process.MemoryUsage.GetReport();
                if (pmReport != null)
                {
                    privateCommit = pmReport.PageFileSizeInBytes;
                }
            }
            return privateCommit;
        }

        private RowAggregateData GetGroupAggregateData(string appName, AppResourceGroupInfo group, ulong totalProcessPrivateCommit)
        {
            Debug.WriteLine("GetGroupAggregateData: app: {0}, group: {1}", appName, group.InstanceId);
            ulong commitLimit = 0;
            ulong totalCommit = 0;
            ulong privateCommit = 0;
            ExecutionStateEx executionState = ExecutionStateEx.Unknown;
            EnergyQuotaStateEx energyState = EnergyQuotaStateEx.Unknown;
            int bgTaskCount = 0;

            try
            {
                AppResourceGroupMemoryReport mReport = group.GetMemoryReport();
                if (mReport != null)
                {
                    commitLimit = mReport.CommitUsageLimit;
                    totalCommit = mReport.TotalCommitUsage;
                    privateCommit = mReport.PrivateCommitUsage;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetMemoryReport: " +ex.ToString());
            }

            // The resource group Private Commit should always be the same (or almost the same) as the PDI Private Commit.
            Debug.WriteLine("Private Commit: ResourceGroup={0:N0} bytes ~= Total PDI={1:N0} bytes", privateCommit, totalProcessPrivateCommit);

            try
            {
                AppResourceGroupStateReport sReport = group.GetStateReport();
                if (sReport != null)
                {
                    executionState = (ExecutionStateEx)sReport.ExecutionState;
                    energyState = (EnergyQuotaStateEx)sReport.EnergyQuotaState;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetStateReport: " + ex.ToString());
            }

            try
            {
                IList<AppResourceGroupBackgroundTaskReport> bgReports = group.GetBackgroundTaskReports();
                if (bgReports != null)
                {
                    bgTaskCount = bgReports.Count;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetBackgroundTaskReports: " + ex.ToString());
            }

            RowAggregateData groupData = new RowAggregateData(
                commitLimit, totalCommit, privateCommit, bgTaskCount, executionState, energyState);

            return groupData;
        }

        #endregion


        #region AddOrUpdateApp

        private bool AddOrUpdateApp(
            AppDiagnosticInfo app, RowAggregateData appData, ExecutionStateEx appExecutionState)
        {
            bool result = false;
            string name = "(unknown)";
            string appId = string.Empty;
            if (app.AppInfo != null && app.AppInfo.DisplayInfo != null)
            {
                appId = app.AppInfo.AppUserModelId;
                name = app.AppInfo.DisplayInfo.DisplayName;
            }

            // Check to see if this app is already in our list, and only add it if it wasn't already there.
            bool isAppFound = false;
            foreach (AppRowInfo existingRow in this)
            {
                if (appId == existingRow.Id)
                {
                    isAppFound = true;
                    break;
                }
            }

            // Don't add apps for resource groups that are not running.
            // Otherwise, for one thing, trying to get the logo would throw.
            if (!isAppFound && appExecutionState != ExecutionStateEx.NotRunning)
            {
                BitmapImage logo = GetLogoFromAppInfo(app);

                // A new app has appeared, so we add it to the list.
                AppRowInfo appRow = new AppRowInfo(
                    app, 
                    appId, logo, name,
                    appData.CommitLimit, appData.TotalCommit, appData.PrivateCommit,
                    appData.ExecutionState, appData.EnergyState, appData.BgTaskCount);
                Add(appRow);
                result = true;
            }
            else
            {
                // For existing rows, we'll update the dynamic state.
                IEnumerable<AppRowInfo> appRows = this.Where(r => r.Id == appId);
                if (appRows != null && appRows.Count() > 0)
                {
                    AppRowInfo existingApp = appRows.First();
                    existingApp.Update(
                        appData.CommitLimit, appData.TotalCommit, appData.PrivateCommit,
                        appData.ExecutionState, appData.EnergyState, appData.BgTaskCount);
                }
            }
            return result;
        }

        #endregion


        #region GetLogoAsync

        private BitmapImage GetLogoFromAppInfo(AppDiagnosticInfo app)
        {
            BitmapImage bitmapImage = new BitmapImage();
            if (app != null && app.AppInfo != null && app.AppInfo.DisplayInfo != null)
            {
                // AppDisplayInfo.GetLogo gets the largest logo defined in the package that fits the given size.
                RandomAccessStreamReference stream = app.AppInfo.DisplayInfo.GetLogo(new Size(64, 64));
                IAsyncOperation<IRandomAccessStreamWithContentType> streamOperation = stream.OpenReadAsync();
                Task<IRandomAccessStreamWithContentType> streamTask = streamOperation.AsTask();
                streamTask.Wait();
                IRandomAccessStreamWithContentType content = streamTask.Result;
                IAsyncAction imageAction = bitmapImage.SetSourceAsync(content);
            }
            return bitmapImage;
        }

        #endregion


        #region UpdateDetails

        public AppProcessesTasks UpdateDetails(AppRowInfo detailsApp)
        {
            Debug.WriteLine($"UpdateDetails: {detailsApp.Name}");

            AppProcessesTasks apt = null;
            List<ProcessInfoDetails> pInfos = new List<ProcessInfoDetails>();
            List<AppResourceGroupBackgroundTaskReport> bgTasks = new List<AppResourceGroupBackgroundTaskReport>();

            // Create an AppInfoDetails from this AppRowInfo.AppDiagnosticInfo.
            AppInfoDetails appDetails = CreateDetailsFromDiagnostics(detailsApp);
            if (appDetails != null)
            {
                // If we have any group data, get that information also.
                if (appDetails.Groups != null && appDetails.Groups.Count > 0)
                {
                    // We start the list with the first group.
                    // We have a hiearchy of group|proc|bgTask, so we could switch the proc|bgTask list
                    // according to the group that's currently selected, but for simplicity, we will
                    // instead always list all the processes and background tasks.
                    foreach (GroupInfoDetails group in appDetails.Groups)
                    {
                        if (group.Processes != null && group.Processes.Count > 0)
                        {
                            foreach (ProcessInfoDetails pInfo in group.Processes)
                            {
                                pInfos.Add(pInfo);
                            }
                        }
                        if (group.BgTasks != null && group.BgTasks.Count > 0)
                        {
                            foreach (AppResourceGroupBackgroundTaskReport bgTask in group.BgTasks)
                            {
                                bgTasks.Add(bgTask);
                            }
                        }
                    }
                }
                apt = new AppProcessesTasks() { AppInfoDetails = appDetails, BgTasks = bgTasks, PInfos = pInfos };
            }
            return apt;
        }

        private AppInfoDetails CreateDetailsFromDiagnostics(AppRowInfo ari)
        {
            Debug.WriteLine("CreateDetailsFromDiagnostics: aumid={0}, name={1}",
                ari.Adi.AppInfo.AppUserModelId, ari.Adi.AppInfo.DisplayInfo.DisplayName);

            AppInfoDetails appDetails = null;
            try
            {
                // Create an AppInfoDetails from this AppRowInfo.AppDiagnosticInfo.
                IList<GroupInfoDetails> groupDetails = new List<GroupInfoDetails>();
                IList<AppResourceGroupInfo> groups = ari.Adi.GetResourceGroups();
                if (groups != null && groups.Count > 0)
                {
                    foreach (AppResourceGroupInfo group in groups)
                    {
                        Debug.WriteLine("group Id: {0}", group.InstanceId);

                        IList<ProcessInfoDetails> pDetails = new List<ProcessInfoDetails>();
                        IList<ProcessDiagnosticInfo> pInfos = group.GetProcessDiagnosticInfos();
                        if (pInfos != null && pInfos.Count > 0)
                        {
                            foreach (ProcessDiagnosticInfo pInfo in pInfos)
                            {
                                TimeSpan kernel = TimeSpan.Zero;
                                TimeSpan user = TimeSpan.Zero;

                                ulong npp = 0;
                                ulong pp = 0;
                                ulong pFault = 0;
                                ulong pFile = 0;
                                ulong pNpp = 0;
                                ulong pPP = 0;
                                ulong ppFile = 0;
                                ulong pVirt = 0;
                                ulong pWSet = 0;
                                ulong ppc = 0;
                                ulong vm = 0;
                                ulong ws = 0;

                                long br = 0;
                                long bw = 0;
                                long ob = 0;
                                long oo = 0;
                                long ro = 0;
                                long wo = 0;

                                ProcessCpuUsageReport pcReport = pInfo.CpuUsage.GetReport();
                                if (pcReport != null)
                                {
                                    kernel = pcReport.KernelTime;
                                    user = pcReport.UserTime;
                                }
                                ProcessMemoryUsageReport pmReport = pInfo.MemoryUsage.GetReport();
                                if (pmReport != null)
                                {
                                    npp = pmReport.NonPagedPoolSizeInBytes;
                                    pp = pmReport.PagedPoolSizeInBytes;
                                    pFault = pmReport.PageFaultCount;
                                    pFile = pmReport.PageFileSizeInBytes;
                                    pNpp = pmReport.PeakNonPagedPoolSizeInBytes;
                                    pPP = pmReport.PeakPagedPoolSizeInBytes;
                                    ppFile = pmReport.PeakPageFileSizeInBytes;
                                    pVirt = pmReport.PeakVirtualMemorySizeInBytes;
                                    pWSet = pmReport.PeakWorkingSetSizeInBytes;
                                    ppc = pmReport.PrivatePageCount;
                                    vm = pmReport.VirtualMemorySizeInBytes;
                                    ws = pmReport.WorkingSetSizeInBytes;
                                }
                                ProcessDiskUsageReport pdReport = pInfo.DiskUsage.GetReport();
                                if (pdReport != null)
                                {
                                    br = pdReport.BytesReadCount;
                                    bw = pdReport.BytesWrittenCount;
                                    ob = pdReport.OtherBytesCount;
                                    oo = pdReport.OtherOperationCount;
                                    ro = pdReport.ReadOperationCount;
                                    wo = pdReport.WriteOperationCount;
                                }

                                ProcessInfoDetails pDetail = new ProcessInfoDetails(
                                    pInfo.ProcessId, pInfo.ExecutableFileName, pInfo.ProcessStartTime,
                                    kernel, user,
                                    npp, pp, pFault, pFile, pNpp, pPP, ppFile, pVirt, pWSet, ppc, vm, ws,
                                    br, bw, ob, oo, ro, wo);
                                pDetails.Add(pDetail);
                            }
                        }

                        AppMemoryUsageLevel usageLevel = AppMemoryUsageLevel.Low;
                        ulong commitLimit = 0;
                        ulong privateCommit = 0;
                        ulong totalCommit = 0;
                        AppResourceGroupExecutionState ex = AppResourceGroupExecutionState.Unknown;
                        AppResourceGroupEnergyQuotaState eq = AppResourceGroupEnergyQuotaState.Unknown;

                        AppResourceGroupMemoryReport mReport = group.GetMemoryReport();
                        AppResourceGroupStateReport sReport = group.GetStateReport();
                        IList<AppResourceGroupBackgroundTaskReport> bgReports = new List<AppResourceGroupBackgroundTaskReport>();
                        bgReports = group.GetBackgroundTaskReports();

                        if (mReport != null)
                        {
                            usageLevel = mReport.CommitUsageLevel;
                            commitLimit = mReport.CommitUsageLimit;
                            privateCommit = mReport.PrivateCommitUsage;
                            totalCommit = mReport.TotalCommitUsage;
                        }
                        if (sReport != null)
                        {
                            ex = sReport.ExecutionState;
                            eq = sReport.EnergyQuotaState;
                        }
                        GroupInfoDetails groupDetail = new GroupInfoDetails(
                            group.InstanceId, group.IsShared,
                            usageLevel, commitLimit, privateCommit, totalCommit,
                            ex, eq,
                            bgReports, pDetails);
                        groupDetails.Add(groupDetail);
                    }

                    // We'll save time and get the cached Logo from the ARI, and get the rest of the data from the ADI.
                    appDetails = new AppInfoDetails(
                        ari.Logo,
                        ari.Adi.AppInfo.AppUserModelId, ari.Adi.AppInfo.Id, ari.Adi.AppInfo.PackageFamilyName,
                        ari.Adi.AppInfo.DisplayInfo.DisplayName, ari.Adi.AppInfo.DisplayInfo.Description,
                        groupDetails);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return appDetails;
        }

        #endregion


        #region Sort

        public void Sort(string column, bool isUserAction)
        {
            List<AppRowInfo> sorted = null;
            switch (column)
            {
                case "Name":
                    sorted = this.OrderBy(a => a.Name).ToList();
                    break;
                case "CommitLimit":
                    sorted = this.OrderBy(a => a.CommitLimit).ToList();
                    break;
                case "TotalCommit":
                    sorted = this.OrderBy(a => a.TotalCommit).ToList();
                    break;
                case "PrivateCommit":
                    sorted = this.OrderBy(a => a.PrivateCommit).ToList();
                    break;
                case "ExecutionState":
                    sorted = this.OrderBy(a => a.ExecutionState).ToList();
                    break;
                case "EnergyQuotaState":
                    sorted = this.OrderBy(a => a.EnergyQuotaState).ToList();
                    break;
                case "BackgroundTaskCount":
                    sorted = this.OrderBy(a => a.BackgroundTaskCount).ToList();
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
