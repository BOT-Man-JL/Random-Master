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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace RandomMaster
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class changeQuestionDialogue : ContentDialog
	{
		Ques thisQues;
		public changeQuestionDialogue(Ques target)
		{
			this.InitializeComponent();
			thisQues = target;
			newTitle.Text = thisQues.question;
		}

		private async void titleModifyWindow_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			if (newTitle.Text != "" && thisQues.question != newTitle.Text)
			{
				thisQues.question = newTitle.Text;
				thisQues.feature = null;
				await App.qm.PeekFeature();
			}
		}

		private void newTitle_KeyUp(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key == Windows.System.VirtualKey.Enter)
			{
				titleModifyWindow_PrimaryButtonClick(null, null);
				Hide();
			}
		}
	}
}
