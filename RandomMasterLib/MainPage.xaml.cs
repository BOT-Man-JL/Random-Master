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
using Windows.UI.Core;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace RandomMaster
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
            MyFrame.Navigate(typeof(ListPage));
           
            this.IconListBox.SelectedIndex = 0;
            //订阅返回键的响应事件
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
            //订阅窗口的导航事件
            MyFrame.Navigated += RootFrame_Navigated;
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
        }
        //返回键的响应事件定义
        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            // 这里面可以任意选择控制哪个Frame 
            // 如果MainPage.xaml中使用了另外的Frame标签进行导航 可在此处获取需要GoBack的Frame
            var rootFrame = MyFrame;
            //var currentPage = rootFrame.CurrentSourcePageType;
            //// ReSharper disable once PossibleNullReferenceException
            //if (currentPage == typeof(ListPage))
            //{
            //    App.Current.Exit();
            //}
            if (rootFrame.CanGoBack)
                rootFrame.GoBack();
            else
                return;
            // 设置指示应用程序已执行请求的后退导航操作
            e.Handled = true;
        }
        //导航事件响应的定义
        private void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // 每次完成导航 确定下是否显示系统后退按钮
            // ReSharper disable once PossibleNullReferenceException
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
              MyFrame.BackStack.Any()
                ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            //汉堡菜单根据当前FRAME的页面进行指示
            if (MyFrame.CurrentSourcePageType == typeof(ListPage))
            {
                this.IconListBox.SelectedIndex = 0;
                this.configListBox.SelectedItem = null;
            }

            else if (MyFrame.CurrentSourcePageType == typeof(Feedback))
            {
                this.IconListBox.SelectedIndex = 1;
                this.configListBox.SelectedItem = null;
            }
            else if (MyFrame.CurrentSourcePageType == typeof(AboutPage))
            {
                this.configListBox.SelectedIndex = 0;
                this.IconListBox.SelectedItem = null; 
            }
        }
        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            this.mySplitView.IsPaneOpen = !this.mySplitView.IsPaneOpen;
        }

        private void IconListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HomeListBoxItem.IsSelected)
            {
                if (this.mySplitView.IsPaneOpen)
                {
                    this.mySplitView.IsPaneOpen = !this.mySplitView.IsPaneOpen;
                }
                configListBox.SelectedItem = null;
                MyFrame.Navigate(typeof(ListPage));
            }
            if (FeedbackListBoxItem.IsSelected)
            {
                if(this.mySplitView.IsPaneOpen)
                {
                    this.mySplitView.IsPaneOpen = !this.mySplitView.IsPaneOpen;
                }
                configListBox.SelectedItem = null;
                MyFrame.Navigate(typeof(Feedback));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MyFrame.CanGoBack)
            {
                MyFrame.GoBack();
            }
        }

        private void configListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (configListBoxItem.IsSelected)
            {
                if (this.mySplitView.IsPaneOpen)
                {
                    this.mySplitView.IsPaneOpen = !this.mySplitView.IsPaneOpen;
                }
                IconListBox.SelectedItem = null;
                MyFrame.Navigate(typeof(AboutPage));
            }
        }
    }
}
