﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace RandomMaster
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class tutorialPage2 : Page
	{
		private Color prevColor;

		public tutorialPage2()
		{
			this.InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			// Set TitleBar Color
			var view = ApplicationView.GetForCurrentView();
			if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
			{
				prevColor = (Color)Application.Current.Resources["SystemAccentColor"];
				var bgColor = Colors.Orange;
				view.TitleBar.BackgroundColor = bgColor;
				view.TitleBar.ButtonBackgroundColor = bgColor;
			}
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);

			// Set TitleBar Color
			var view = ApplicationView.GetForCurrentView();
			if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
			{
				view.TitleBar.BackgroundColor = prevColor;
				view.TitleBar.ButtonBackgroundColor = prevColor;
			}
		}

		private void exitBtn_Click(object sender, RoutedEventArgs e)
		{
			Config.tutorialProcess = 0;
			Config.SaveConfig();

			this.Frame.Navigate(typeof(MainPage));
		}

		private async void cortanaBtn_Click(object sender, RoutedEventArgs e)
		{
			await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-cortana://"));
		}

		private void testBtn_Click(object sender, RoutedEventArgs e)
		{
			this.Frame.Navigate(typeof(tutorialPage3));
		}
	}
}
