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
    public sealed partial class welcomePageAll : Page
    {
        private bool isChecked1 = false;
        private bool isChecked2 = false;
        private bool isChecked3 = false;
        public welcomePageAll()
        {
            this.InitializeComponent();

        }

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(myFlipView.SelectedIndex == 0&&(!isChecked1))
            {
                frame1.Navigate(typeof(welcomePage));
                isChecked1 = true;
            }
            if(myFlipView.SelectedIndex == 1&&(!isChecked2))
            {
                frame2.Navigate(typeof(welcomepage2));
                isChecked2 = true;
            }
            if(myFlipView.SelectedIndex == 2&&(!isChecked3))
            {
                frame3.Navigate(typeof(welcomepage3));
                isChecked3 = true;
            }
        }
    }
}
