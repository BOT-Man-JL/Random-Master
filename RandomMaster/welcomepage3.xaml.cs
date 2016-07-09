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
using Windows.UI.Xaml.Media.Animation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace RandomMaster
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class welcomepage3 : Page
	{
		public welcomepage3()
		{
			this.InitializeComponent();
			((Storyboard)this.Resources["myStoryBoard"]).Begin();
        }

		private void DoubleAnimation_Completed(object sender, object e)
		{
			((Storyboard)this.Resources["myStoryBoard"]).Pause();
		}

		private void startBtn_Click(object sender, RoutedEventArgs e)
		{
			Config.isFirstTime = false;
			Config.SaveConfig();

			(Window.Current.Content as Frame).Navigate(typeof(MainPage));
		}

        //private void TutotrialBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    if (tutorialBtn.Visibility == Visibility.Visible)
        //        tutorialBtn.Visibility = Visibility.Collapsed;
        //    else
        //        tutorialBtn.Visibility = Visibility.Visible;
        //}

        private void ballAnimation_Completed(object sender, object e)
        {
            var rootFrame = (Window.Current.Content as Frame);
            rootFrame.Navigate(typeof(tutorialPage1));
        }

        private void tutorialBtn_Click(object sender, RoutedEventArgs e)
		{
			Config.isFirstTime = false;
			Config.tutorialProcess = 1;
			Config.SaveConfig();

			this.ball.Visibility = Visibility.Visible;
            ((Storyboard)this.Resources["tutorialBegin"]).Begin();
        }
    }
}
