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
	public sealed partial class AddOptionDialogue : ContentDialog
	{
		Ques thisQues;
		public AddOptionDialogue(Ques target)
		{
			thisQues = target;
			this.InitializeComponent();
		}

		private void addOptionWindow_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			if (!(newOption.Text == ""))
			{
				App.qm.SetQues(thisQues.question, new List<string> { newOption.Text }, null);
				App.qm.Save();
			}
		}

		//  Remark KeyDown Hell
		private void newOption_KeyUp(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key == Windows.System.VirtualKey.Enter)
			{
				addOptionWindow_PrimaryButtonClick(null, null);
				Hide();
			}
		}
	}
}
