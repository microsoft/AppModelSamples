using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TestRestart
{
    sealed partial class App : Application
    {

        #region Standard stuff

        public static ImagesViewModel Images { get; set; }

        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            Images = new ImagesViewModel();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                Window.Current.Activate();
            }
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        #endregion


        #region OnActivated

        protected override void OnActivated(IActivatedEventArgs args)
        {
            switch (args.Kind)
            {
                case ActivationKind.Launch:
                    LaunchActivatedEventArgs launchArgs = args as LaunchActivatedEventArgs;
                    string argString = launchArgs.Arguments;

                    Frame rootFrame = Window.Current.Content as Frame;
                    if (rootFrame == null)
                    {
                        rootFrame = new Frame();
                        Window.Current.Content = rootFrame;
                    }
                    rootFrame.Navigate(typeof(MainPage), argString);
                    Window.Current.Activate();
                    break;
            }
        }

        #endregion
    }
}
