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
    public sealed partial class welcomePage : Page
    {
        public welcomePage()
        {
            //隐藏状态栏
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var i = Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
            }
            this.InitializeComponent();
            ((Storyboard)this.Resources["myStoryBoard"]).Begin();
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            ((Storyboard)this.Resources["myStoryBoard"]).Resume();

        }

        private void DoubleAnimation_Completed(object sender, object e)
        {
            ((Storyboard)this.Resources["myStoryBoard"]).Pause();
        }

        private void DoubleAnimation_Completed_1(object sender, object e)
        {
            Frame.Navigate(typeof(welcomepage2));
        }
    }
}
