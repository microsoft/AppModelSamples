using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using TaskMonitor.Controls;
using TaskMonitor.ViewModels;
using Windows.Devices.Power;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using static TaskMonitor.NativeMethods;

namespace TaskMonitor
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {

        #region Fields & properties

        private const int LIST_HEIGHT_OFFSET = 200;
        private const int SCROLLBAR_WIDTH_OFFSET = 24;
        private const int DEFAULT_APPLIST_TIMER_INTERVAL = 5;
        private const int DEFAULT_PROCESSLIST_TIMER_INTERVAL = 30;
        private const string DEFAULT_PIVOT_NAME = "Processes";

        private DispatcherTimer processUpdateTimer;
        private DispatcherTimer appUpdateTimer;
        private DispatcherTimer systemUpdateTimer;
        private ProcRowInfoCollection processes = new ProcRowInfoCollection();
        private AppRowInfoCollection apps = new AppRowInfoCollection();
        private bool isFocusOnDetails = false;
        private AppRowInfo detailsApp;
        private AppProcessesTasks appProcessTasks;
        private string processSortColumn = "ExecutableFileName";
        private string appSortColumn = "Name";

        private const double GB = 1024 * 1024 * 1024;
        private const double MBPS = 1000 * 1000;
        private bool isStaticSystemInfoInitialized;
        private bool isFrozen;

        public StaticSystemInfo StaticSystemData { get; internal set; }
        public DynamicSystemInfo DynamicSystemData { get; internal set; }

        private DiagnosticAccessStatus accessStatus;
        public DiagnosticAccessStatus AccessStatus
        {
            get { return accessStatus; }
            set
            {
                if (accessStatus != value)
                {
                    accessStatus = value;
                    NotifyPropertyChanged("AccessStatus");
                }
            }
        }

        private DateTime processLastUpdate;
        public DateTime ProcessLastUpdate
        {
            get { return processLastUpdate; }
            set
            {
                if (processLastUpdate != value)
                {
                    processLastUpdate = value;
                    NotifyPropertyChanged("ProcessLastUpdate");
                }
            }
        }

        private DateTime appLastUpdate;
        public DateTime AppLastUpdate
        {
            get { return appLastUpdate; }
            set
            {
                if (appLastUpdate != value)
                {
                    appLastUpdate = value;
                    NotifyPropertyChanged("AppLastUpdate");
                }
            }
        }

        private int processPollingInterval;
        public int ProcessPollingInterval
        {
            get { return processPollingInterval; }
            set
            {
                if (processPollingInterval != value)
                {
                    processPollingInterval = value;
                    NotifyPropertyChanged("ProcessPollingInterval");
                }
            }
        }

        private int appPollingInterval;
        public int AppPollingInterval
        {
            get { return appPollingInterval; }
            set
            {
                if (appPollingInterval != value)
                {
                    appPollingInterval = value;
                    NotifyPropertyChanged("AppPollingInterval");
                }
            }
        }

        private int systemPollingInterval;
        public int SystemPollingInterval
        {
            get { return systemPollingInterval; }
            set
            {
                if (systemPollingInterval != value)
                {
                    systemPollingInterval = value;
                    NotifyPropertyChanged("SystemPollingInterval");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion


        #region Init

        public MainPage()
        {
            AccessStatus = DiagnosticAccessStatus.Unspecified;
            ProcessLastUpdate = DateTime.MinValue;
            AppLastUpdate = DateTime.MinValue;
            ProcessPollingInterval = DEFAULT_PROCESSLIST_TIMER_INTERVAL;
            AppPollingInterval = DEFAULT_APPLIST_TIMER_INTERVAL;
            SystemPollingInterval = DEFAULT_PROCESSLIST_TIMER_INTERVAL;
            InitializeComponent();
            versionText.Text = App.VersionString;
            contentPanel.DataContext = this;

            App.ActivateDisplayRequest();
            ApplicationView.GetForCurrentView().Title = DEFAULT_PIVOT_NAME;
        }

        // Size the main ListBoxes dynamically when the Page size changes.
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double windowWidth = Window.Current.Bounds.Width;
            double windowHeight = Window.Current.Bounds.Height;

            processHeaderGrid.Width = windowWidth;
            processListView.Width = windowWidth - SCROLLBAR_WIDTH_OFFSET;
            processListView.Height = windowHeight - LIST_HEIGHT_OFFSET;

            appHeaderGrid.Width = windowWidth;
            appListView.Width = windowWidth - SCROLLBAR_WIDTH_OFFSET;
            appListView.Height = windowHeight - LIST_HEIGHT_OFFSET;
        }

        async protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AccessStatus = await apps.GetAccessStatus();
            processListView.ItemsSource = processes;
            appListView.ItemsSource = apps;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            StopAppPolling();
            StopProcessPolling();
            StopSystemPolling();
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        #region Pivot_SelectionChanged

        private void mainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem selectedPivot = mainPivot.SelectedItem as PivotItem;

            App.AnalyticsWriteLine("mainPivot_SelectionChanged", "PivotItem.Name", selectedPivot.Name);

            // Make the active pivot image colored, and inactive items greyed.
            // We should probably do this with VisualStateManager, but this seems to be faster.
            foreach (PivotItem item in mainPivot.Items)
            {
                if (item == selectedPivot)
                {
                    TabHeader header = item.Header as TabHeader;
                    if (header != null)
                    {
                        header.SetSelectedItem(true);
                    }
                }
                else
                {
                    TabHeader header = item.Header as TabHeader;
                    if (header != null)
                    {
                        header.SetSelectedItem(false);
                    }
                }
            }
            ApplicationView.GetForCurrentView().Title = selectedPivot.Name;

            // Only run the timer for the active pivot.
            switch (selectedPivot.Name)
            {
                case "Processes":
                    isFocusOnDetails = false;
                    StopAppPolling();
                    StopSystemPolling();
                    UpdateProcessData();
                    InitializeProcessPolling();
                    break;
                case "Apps":
                    isFocusOnDetails = false;
                    StopProcessPolling();
                    StopSystemPolling();
                    UpdateAppData();
                    InitializeAppPolling();
                    break;
                case "Details":
                    StopProcessPolling();
                    StopSystemPolling();

                    // Get the selected app from the Apps pivot.
                    AppRowInfo app = appListView.SelectedItem as AppRowInfo;
                    if (app != null && app.Adi != null)
                    {
                        detailsApp = app;
                        isFocusOnDetails = true;
                        UpdateDetails();
                    }

                    // We'll start the Details pivot with polling frozen.
                    //InitializeAppPolling();
                    StopAppPolling();
                    freezeButton.Content = "Unfreeze";
                    isFrozen = true;

                    break;
                case "System":
                    isFocusOnDetails = false;
                    StopAppPolling();
                    StopProcessPolling();
                    if (!isStaticSystemInfoInitialized)
                    {
                        GetStaticSystemInfo();
                        isStaticSystemInfoInitialized = true;
                    }
                    UpdateDynamicSystemData();
                    InitializeSystemPolling();
                    break;
                case "Settings":
                    isFocusOnDetails = false;
                    StopAppPolling();
                    StopProcessPolling();
                    StopSystemPolling();
                    break;
            }
        }

        #endregion


        #region Polling

        private void InitializeProcessPolling()
        {
            if (processUpdateTimer != null)
            {
                processUpdateTimer.Stop();
                processUpdateTimer = null;
            }
            if (ProcessPollingInterval != 0)
            {
                processUpdateTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(ProcessPollingInterval) };
                processUpdateTimer.Tick += OnUpdateProcessTimerTick;
                processUpdateTimer.Start();
            }
        }

        private void StopProcessPolling()
        {
            if (processUpdateTimer != null)
            {
                processUpdateTimer.Stop();
                processUpdateTimer = null;
                processes.Clear();
            }
        }

        private void OnUpdateProcessTimerTick(object sender, object e)
        {
            UpdateProcessData();
        }

        private void InitializeAppPolling()
        {
            if (appUpdateTimer != null)
            {
                appUpdateTimer.Stop();
                appUpdateTimer = null;
            }
            if (AppPollingInterval != 0)
            {
                appUpdateTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(AppPollingInterval) };
                appUpdateTimer.Tick += OnUpdateAppUpdateTimerTick;
                appUpdateTimer.Start();
            }
        }

        private void StopAppPolling()
        {
            if (appUpdateTimer != null)
            {
                appUpdateTimer.Stop();
                appUpdateTimer = null;
                apps.Clear();
            }
        }

        private void OnUpdateAppUpdateTimerTick(object sender, object e)
        {
            UpdateAppData();
        }

        private void InitializeSystemPolling()
        {
            if (systemUpdateTimer != null)
            {
                systemUpdateTimer.Stop();
                systemUpdateTimer = null;
            }
            if (SystemPollingInterval != 0)
            {
                systemUpdateTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(SystemPollingInterval) };
                systemUpdateTimer.Tick += OnUpdateSystemTimerTick;
                systemUpdateTimer.Start();
            }
        }

        private void StopSystemPolling()
        {
            if (systemUpdateTimer != null)
            {
                systemUpdateTimer.Stop();
                systemUpdateTimer = null;
            }
        }

        private void OnUpdateSystemTimerTick(object sender, object e)
        {
            UpdateDynamicSystemData();
        }

        #endregion


        #region UpdateData

        private void UpdateProcessData()
        {
            processes.Update();
            processes.Sort(processSortColumn, false);
            ProcessLastUpdate = DateTime.Now;
        }

        private void UpdateAppData()
        {
            if (isFocusOnDetails)
            {
                UpdateDetails();
            }
            else
            {
                bool isAppAdded = apps.Update();
                if (isAppAdded)
                {
                    apps.Sort(appSortColumn, false);
                }
                AppLastUpdate = DateTime.Now;
            }
        }

        private void UpdateDetails()
        {
            appProcessTasks = apps.UpdateDetails(detailsApp);
            if (appProcessTasks != null)
            {
                appInfoGrid.DataContext = appProcessTasks.AppInfoDetails;
                groupListView.ItemsSource = appProcessTasks.AppInfoDetails.Groups;
                groupsText.Text = $"Resource Groups ({appProcessTasks.AppInfoDetails.Groups.Count})";
                bgTaskReportsText.Text = $"BackgroundTaskReports ({appProcessTasks.BgTasks.Count})";
                processesText.Text = $"Processes ({appProcessTasks.PInfos.Count})";
                bgListView.ItemsSource = appProcessTasks.BgTasks;
                procDetailsListView.ItemsSource = appProcessTasks.PInfos;
            }
            else
            {
                // If the selected app is terminated, we'll empty out the UI.
                appInfoGrid.DataContext = null;
                groupListView.ItemsSource = null;
                groupsText.Text = "Resource Groups";
                bgTaskReportsText.Text = "BackgroundTaskReports";
                processesText.Text = "Processes";
                bgListView.ItemsSource = null;
                procDetailsListView.ItemsSource = null;
            }
        }


        #endregion


        #region Column sorting

        private void processHeaderColumn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                processSortColumn = (string)button.Tag;
                processes.Sort(processSortColumn, true);
            }
        }

        private void appHeaderColumn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                appSortColumn = (string)button.Tag;
                apps.Sort(appSortColumn, true);
            }
        }

        #endregion


        #region Static system info

        private void GetStaticSystemInfo()
        {
           StaticSystemData = new StaticSystemInfo();

            try
            {
                EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
                StaticSystemData.MachineName = deviceInfo.FriendlyName;
                StaticSystemData.MakeModel = $"{deviceInfo.SystemManufacturer}, {deviceInfo.SystemProductName}";
                StaticSystemData.FormFactor = App.FormFactor.ToString();

                string familyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
                ulong v = ulong.Parse(familyVersion);
                ulong v1 = (v & 0xFFFF000000000000L) >> 48;
                ulong v2 = (v & 0x0000FFFF00000000L) >> 32;
                ulong v3 = (v & 0x00000000FFFF0000L) >> 16;
                ulong v4 = (v & 0x000000000000FFFFL);
                string deviceVersion = $"{v1}.{v2}.{v3}.{v4}";
                StaticSystemData.OSVersion = deviceVersion;

                SYSTEM_INFO sysInfo = new SYSTEM_INFO();
                GetSystemInfo(ref sysInfo);
                StaticSystemData.LogicalProcessors = sysInfo.dwNumberOfProcessors.ToString();
                StaticSystemData.Processor = $"{sysInfo.wProcessorArchitecture}, level {sysInfo.wProcessorLevel}, rev {sysInfo.wProcessorRevision}";
                StaticSystemData.PageSize = sysInfo.dwPageSize.ToString();
            }
            catch (Exception ex)
            {
                App.AnalyticsWriteLine("MainPage.GetStaticSystemInfo", "Exception", ex.Message);
            }
            staticDataGrid.DataContext = StaticSystemData;
        }

        #endregion


        #region Dynamic system info

        private void UpdateDynamicSystemData()
        {
            DynamicSystemData = new DynamicSystemInfo();

            try
            {
                MEMORYSTATUSEX memoryStatus = new MEMORYSTATUSEX();
                GlobalMemoryStatusEx(memoryStatus);
                DynamicSystemData.PhysicalMemory = $"total = {memoryStatus.ullTotalPhys / GB:N2} GB, available = {memoryStatus.ullAvailPhys / GB:N2} GB";
                DynamicSystemData.PhysicalPlusPagefile = $"total = {memoryStatus.ullTotalPageFile / GB:N2} GB, available = {memoryStatus.ullAvailPageFile / GB:N2} GB";
                DynamicSystemData.VirtualMemory = $"total = {memoryStatus.ullTotalVirtual / GB:N2} GB, available = {memoryStatus.ullAvailVirtual / GB:N2} GB";
                ulong pageFileOnDisk = memoryStatus.ullTotalPageFile - memoryStatus.ullTotalPhys;
                DynamicSystemData.PagefileOnDisk = $"{pageFileOnDisk / GB:N2} GB";
                DynamicSystemData.MemoryLoad = $"{memoryStatus.dwMemoryLoad}%";
            }
            catch (Exception ex)
            {
                App.AnalyticsWriteLine("MainPage.UpdateDynamicSystemData", "MEMORYSTATUSEX", ex.Message);
            }

            bool isBatteryAvailable = true;
            try
            {
                SYSTEM_POWER_STATUS powerStatus = new SYSTEM_POWER_STATUS();
                GetSystemPowerStatus(ref powerStatus);
                DynamicSystemData.ACLineStatus = powerStatus.ACLineStatus.ToString();

                DynamicSystemData.BatteryChargeStatus = $"{powerStatus.BatteryChargeStatus:G}";
                if (powerStatus.BatteryChargeStatus == BatteryFlag.NoSystemBattery
                    || powerStatus.BatteryChargeStatus == BatteryFlag.Unknown)
                {
                    isBatteryAvailable = false;
                    DynamicSystemData.BatteryLife = "n/a";
                }
                else
                {
                    DynamicSystemData.BatteryLife = $"{powerStatus.BatteryLifePercent}%";
                }
                DynamicSystemData.BatterySaver = powerStatus.BatterySaver.ToString();
            }
            catch (Exception ex)
            {
                App.AnalyticsWriteLine("MainPage.UpdateDynamicSystemData", "SYSTEM_POWER_STATUS", ex.Message);
            }

            if (isBatteryAvailable)
            {
                try
                {
                    Battery battery = Battery.AggregateBattery;
                    BatteryReport batteryReport = battery.GetReport();
                    DynamicSystemData.ChargeRate = $"{batteryReport.ChargeRateInMilliwatts:N0} mW";
                    DynamicSystemData.Capacity = 
                        $"design = {batteryReport.DesignCapacityInMilliwattHours:N0} mWh, " +
                        $"full = {batteryReport.FullChargeCapacityInMilliwattHours:N0} mWh, " +
                        $"remaining = {batteryReport.RemainingCapacityInMilliwattHours:N0} mWh";
                }
                catch (Exception ex)
                {
                    App.AnalyticsWriteLine("MainPage.UpdateDynamicSystemData", "BatteryReport", ex.Message);
                }
            }
            else
            {
                DynamicSystemData.ChargeRate = "n/a";
                DynamicSystemData.Capacity = "n/a";
            }

            try
            {
                ulong freeBytesAvailable;
                ulong totalNumberOfBytes;
                ulong totalNumberOfFreeBytes;

                // You can only specify a folder path that this app can access, but you can
                // get full disk information from any folder path.
                IStorageFolder appFolder = ApplicationData.Current.LocalFolder;
                GetDiskFreeSpaceEx(appFolder.Path, out freeBytesAvailable, out totalNumberOfBytes, out totalNumberOfFreeBytes);
                DynamicSystemData.TotalDiskSize = $"{totalNumberOfBytes / GB:N2} GB";
                DynamicSystemData.DiskFreeSpace = $"{freeBytesAvailable / GB:N2} GB";
            }
            catch (Exception ex)
            {
                App.AnalyticsWriteLine("MainPage.UpdateDynamicSystemData", "GetDiskFreeSpaceEx", ex.Message);
            }

            try
            {
                IntPtr infoPtr = IntPtr.Zero;
                uint infoLen = (uint)Marshal.SizeOf<FIXED_INFO>();
                int ret = -1;

                while (ret != ERROR_SUCCESS)
                {
                    infoPtr = Marshal.AllocHGlobal(Convert.ToInt32(infoLen));
                    ret = GetNetworkParams(infoPtr, ref infoLen);
                    if (ret == ERROR_BUFFER_OVERFLOW)
                    {
                        // Try again with a bigger buffer.
                        Marshal.FreeHGlobal(infoPtr);
                        continue;
                    }
                }

                FIXED_INFO info = Marshal.PtrToStructure<FIXED_INFO>(infoPtr);
                DynamicSystemData.DomainName = info.DomainName;

                string nodeType = string.Empty;
                switch (info.NodeType)
                {
                    case BROADCAST_NODETYPE:
                        nodeType = "Broadcast";
                        break;
                    case PEER_TO_PEER_NODETYPE:
                        nodeType = "Peer to Peer";
                        break;
                    case MIXED_NODETYPE:
                        nodeType = "Mixed";
                        break;
                    case HYBRID_NODETYPE:
                        nodeType = "Hybrid";
                        break;
                    default:
                        nodeType = $"Unknown ({info.NodeType})";
                        break;
                }
                DynamicSystemData.NodeType = nodeType;
            }
            catch (Exception ex)
            {
                App.AnalyticsWriteLine("MainPage.UpdateDynamicSystemData", "GetNetworkParams", ex.Message);
            }

            try
            {
                ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
                DynamicSystemData.ConnectedProfile = profile.ProfileName;

                NetworkAdapter internetAdapter = profile.NetworkAdapter;
                DynamicSystemData.IanaInterfaceType = $"{(IanaInterfaceType)internetAdapter.IanaInterfaceType}";
                DynamicSystemData.InboundSpeed = $"{internetAdapter.InboundMaxBitsPerSecond / MBPS:N0} Mbps";
                DynamicSystemData.OutboundSpeed = $"{internetAdapter.OutboundMaxBitsPerSecond / MBPS:N0} Mbps";

                IReadOnlyList<HostName> hostNames = NetworkInformation.GetHostNames();
                HostName connectedHost = hostNames.Where
                    (h => h.IPInformation != null
                    && h.IPInformation.NetworkAdapter != null
                    && h.IPInformation.NetworkAdapter.NetworkAdapterId == internetAdapter.NetworkAdapterId)
                    .FirstOrDefault();
                if (connectedHost != null)
                {
                    DynamicSystemData.HostAddress = connectedHost.CanonicalName;
                    DynamicSystemData.AddressType = connectedHost.Type.ToString();
                }
            }
            catch (Exception ex)
            {
                App.AnalyticsWriteLine("MainPage.UpdateDynamicSystemData", "GetInternetConnectionProfile", ex.Message);
            }

            dynamicDataGrid.DataContext = DynamicSystemData;
        }


        #endregion


        #region FreezeUpdate

        private void freezeButton_Click(object sender, RoutedEventArgs e)
        {
            isFrozen = !isFrozen;
            if (isFrozen)
            {
                StopAppPolling();
                freezeButton.Content = "Unfreeze";
            }
            else
            {
                InitializeAppPolling();
                freezeButton.Content = "Freeze";
            }
        }

        #endregion

    }
}
