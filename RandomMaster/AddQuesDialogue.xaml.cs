using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上进行了说明

namespace RandomMaster
{
	public sealed partial class AddQuesDialogue : ContentDialog
	{
		public AddQuesDialogue(string text)
		{
			this.InitializeComponent();
			if (text != null)
				newOues.Text = text;
		}

		private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			if (newOues.Text != "")
			{
				// Todo: Detect Question logic if Y-or-N / OR
				App.qm.SetQues(newOues.Text, new List<string> { }, null);
				App.qm.Save();
				await App.qm.PeekFeature();
			}
		}

		private void newOues_KeyUp(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key == Windows.System.VirtualKey.Enter)
			{
				ContentDialog_PrimaryButtonClick(null, null);
				Hide();
			}
		}
	}
}
