using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace RandomMaster
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        int clickcount = 0;
        public AboutPage()
        {
            this.InitializeComponent();
            easterEgg.IsOn = Config.isEEon;
			cortanaFeedback.IsOn = Config.isFBon;
			cortanaRC.IsOn = Config.isRCon;
			cortanaRC.IsEnabled = Config.isFBon;

			if (!Config.isVcdInstalled)
				InstallVCD.Visibility = Visibility.Visible;
		}

        private void easterEgg_Toggled(object sender, RoutedEventArgs e)
        {
            Config.isEEon = easterEgg.IsOn;
            Config.SaveConfig();
		}

		private void cortanaFeedback_Toggled(object sender, RoutedEventArgs e)
		{
			Config.isFBon = cortanaFeedback.IsOn;
			cortanaRC.IsEnabled = Config.isFBon;
			Config.SaveConfig();
		}

		private void cortanaRC_Toggled(object sender, RoutedEventArgs e)
		{
			Config.isRCon = cortanaRC.IsOn;
			Config.SaveConfig();
		}

		private void reTutorial_Click(object sender, RoutedEventArgs e)
		{
			// Enter Tutorial Mode
			Config.tutorialProcess = 1;
			Config.SaveConfig();

			// Navigate to TutorialPage
			var rootFrame = (Window.Current.Content as Frame);
			rootFrame.Navigate(typeof(tutorialPage1));

			// Clear BackStack
			rootFrame.BackStack.Clear();
			SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility
				= AppViewBackButtonVisibility.Collapsed;
		}

		private void reWelcomePage_Click(object sender, RoutedEventArgs e)
        {
			// Navigate to WelcomePage
			var rootFrame = (Window.Current.Content as Frame);
			rootFrame.Navigate(typeof(welcomePageAll), null);

			// Clear BackStack
			rootFrame.BackStack.Clear();
			SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility
				= AppViewBackButtonVisibility.Collapsed;
		}

        private void Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            clickcount++;
            if (clickcount == 3)
            {
                this.Frame.Navigate(typeof(FeatureTest));
                clickcount = 0;
            }
        }

		private async void InstallVCD_Click(object sender, RoutedEventArgs e)
		{
			InstallVCD.IsEnabled = false;

			if (await Config.InstallVcdFile())
			{
				Config.isVcdInstalled = true;
				InstallVCD.Visibility = Visibility.Collapsed;
			}

			InstallVCD.IsEnabled = true;
		}
    }
}
