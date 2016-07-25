using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Popups;
using Windows.System.Profile;
using Windows.Graphics.Display;
using Windows.Foundation.Metadata;
using System.Threading.Tasks;

namespace RandomMaster
{
	/// <summary>
	/// 提供特定于应用程序的行为，以补充默认的应用程序类。
	/// </summary>
	sealed partial class App : Application
	{
		// Shared by UI related stuff
		public static QuesManager qm;

		public App()
		{
			InitializeComponent();
		}

		private async Task Init()
		{
			Config.LoadConfig();
			qm = new QuesManager();

			this.Suspending += OnSuspending;
			//this.Resuming += OnResuming;

			// Set TitleBar Color
			var view = ApplicationView.GetForCurrentView();
			if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
			{
				if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
					await StatusBar.GetForCurrentView().HideAsync();
			}
		}

		protected override async void OnLaunched(LaunchActivatedEventArgs e)
		{
#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				this.DebugSettings.EnableFrameRateCounter = true;
			}
#endif
			await Init();

			Frame rootFrame = Window.Current.Content as Frame;
			if (rootFrame == null)
			{
				rootFrame = new Frame();
				rootFrame.NavigationFailed += OnNavigationFailed;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
					// Do nothing...
				}
				Window.Current.Content = rootFrame;
			}

			if (rootFrame.Content == null)
			{
				if (Config.isFirstTime)
					rootFrame.Navigate(typeof(welcomePageAll), e.Arguments);
				else if (Config.tutorialProcess != 0)
					rootFrame.Navigate(typeof(tutorialPage2));
				else
					rootFrame.Navigate(typeof(MainPage), e.Arguments);
			}
			Window.Current.Activate();

			// VCD Installation
			var flag = await Config.InstallVcdFile();
			if (!Config.isVcdInstalled && flag)
			{
				Config.isVcdInstalled = true;
				Config.SaveConfig();
			}

			if (!Config.isVcdInstalled && Config.isFirstTime)
			{
				var StringLoader = new Windows.ApplicationModel.Resources.ResourceLoader();
				MessageDialog error = new MessageDialog(StringLoader.GetString("App_VcdFailure"));
				await error.ShowAsync();
			}
		}

		protected override async void OnActivated(IActivatedEventArgs args)
		{
			base.OnActivated(args);
			await Init();

			var rootFrame = Window.Current.Content as Frame;
			if (rootFrame == null)
			{
				rootFrame = new Frame();
				rootFrame.NavigationFailed += OnNavigationFailed;

				Window.Current.Content = rootFrame;
			}

			if (args.Kind == ActivationKind.Protocol)
			{
				var commandArgs = args as ProtocolActivatedEventArgs;
				var decoder = new WwwFormUrlDecoder(commandArgs.Uri.Query);
				var arg = decoder.GetFirstValueByName("LaunchContext");

				if (arg == "TutorialDone")
					rootFrame.Navigate(typeof(tutorialPage3));
				else
					await Windows.System.Launcher.LaunchUriAsync(new Uri(arg));
			}

			if (rootFrame.Content == null)
				rootFrame.Navigate(typeof(MainPage));

			Window.Current.Activate();
		}

		void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception("Failed to load Page: " + e.SourcePageType.FullName);
		}

		private void OnSuspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();
			deferral.Complete();
		}
	}
}
