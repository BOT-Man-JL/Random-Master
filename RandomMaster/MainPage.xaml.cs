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
using System.Collections.ObjectModel;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media.Animation;
using Windows.Storage;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace RandomMaster
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class MainPage : Page
	{

		private bool isWideMode = false;

		public NotificationPoint redpoint = Config.redpoint;

		//private void CurrentWindow_SizeChanged(object sender,Windows.UI.Core.WindowSizeChangedEventArgs e)
		//{
		//    if (e.Size.Width >= 720)
		//        VisualStateManager.GoToState(this, "WideState", false);
		//    else
		//        VisualStateManager.GoToState(this, "DefaultState", false);
		//}

		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			//if((string)e.Parameter!="")
			//{
			//    QuesManager.isCortanaQuest = true;
			//    MyFrame.Navigate(typeof(ListPage), e.Parameter);
			//}
			//else
			//    MyFrame.Navigate(typeof(ListPage));
			// ObservableCollection<Ques> Queses = new ObservableCollection<Ques>();

			var StringLoader = new Windows.ApplicationModel.Resources.ResourceLoader();
			MessageDialog error = new MessageDialog(StringLoader.GetString("App_NoSuchQuestion"));
			Ques thisQues = null;

			if ((string)e.Parameter != null && (string)e.Parameter != "")
			{
				foreach (Ques s in App.qm.queses)
				{
					// Remark: Using NLP to match (but this part has been removed)
					if (s.question == (string)e.Parameter)
					{
						thisQues = s;
						break;
					}
				}

				if (thisQues != null)
					MyFrame.Navigate(typeof(Option), thisQues);
				else
				{
					await error.ShowAsync();
					var newquestbox = new AddQuesDialogue((string)e.Parameter);
					await newquestbox.ShowAsync();
				}
			}
		}

		public MainPage()
		{
			this.InitializeComponent();
			this.IconListBox.SelectedIndex = 0;

			//订阅返回键的响应事件
			SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;

			//订阅窗口的导航事件
			MyFrame.Navigated += RootFrame_Navigated;
			MyFrame.Navigate(typeof(ListPage));
			myFrame2.Navigate(typeof(waitPage));

			//敞口尺寸变化事件句柄
			this.SizeChanged += CurrentWindow_SizeChanged;
			if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
			{
				this.mySplitView.OpenPaneLength = 180;
			}

			// Achievement #0 welcome
			AccomplishmentsManager.ReportAccomplishment(0);
		}

		//超变态的窗口尺寸响应函数
		private void CurrentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			//宽屏尺寸
			if (e.NewSize.Width >= 720 && e.NewSize.Width < 1280)
			{
				if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
					this.mySplitView.OpenPaneLength = 180;
				VisualStateManager.GoToState(this, "WideState", true);
				isWideMode = true;
				Config.isSuperWideMode = false;
				sideFrameGrid.MaxWidth = 0;
				//从超宽屏切回时如果frame2有option列表，则切换回来时显示option列表
				if (myFrame2.CurrentSourcePageType == typeof(Option))
				{
					DependencyObject nowQues = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(myFrame2, 0), 0);
					MyFrame.Navigate(typeof(Option), ((Option)nowQues).thisQues);
				}
				myFrame2.Navigate(typeof(waitPage));
			}
			//小屏尺寸
			if (e.NewSize.Width < 720)
			{

				this.mySplitView.OpenPaneLength = 280;
				VisualStateManager.GoToState(this, "DefaultState", true);
				isWideMode = false;
				Config.isSuperWideMode = false;
				sideFrameGrid.MaxWidth = 0;
				//从超宽屏切回时如果frame2有option列表，则切换回来时显示option列表
				if (myFrame2.CurrentSourcePageType == typeof(Option))
				{
					DependencyObject nowQues = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(myFrame2, 0), 0);
					MyFrame.Navigate(typeof(Option), ((Option)nowQues).thisQues);
				}
				myFrame2.Navigate(typeof(waitPage));
			}
			//超大屏尺寸！
			if (e.NewSize.Width >= 1280)
			{
				isWideMode = true;
				Config.isSuperWideMode = true;
				if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
					this.mySplitView.OpenPaneLength = 180;
				if (MyFrame.CurrentSourcePageType == typeof(ListPage) || MyFrame.CurrentSourcePageType == typeof(Option))
					sideFrameGrid.MaxWidth = int.MaxValue;
				//切换到超宽屏前，若显示的是某问题的选项页面，切换后将此页面显示到旁边，中间显示问题列表
				if (MyFrame.CurrentSourcePageType == typeof(Option))
				{
					DependencyObject nowQues = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(MyFrame, 0), 0);
					myFrame2.Navigate(typeof(Option), ((Option)nowQues).thisQues);
					MyFrame.Navigate(typeof(ListPage));
				}

				VisualStateManager.GoToState(this, "WideState", true);
			}
			//  I suppose no need to collapse (since too much bug on diverse devices)
			//if (e.NewSize.Height >= 720)
			//    this.menuBackgroungLOGO.Visibility = Visibility.Visible;
			//else
			//    this.menuBackgroungLOGO.Visibility = Visibility.Collapsed;
			//menuBackgroungLOGO.Margin = new Thickness(50, Window.Current.CoreWindow.Bounds.Height - 110, -256.934, 0);
			if (mySplitView.IsPaneOpen)
			{
				((Storyboard)this.Resources["HamburgerMenuImageControl"]).Begin();
			}
			else
			{
				((Storyboard)this.Resources["HamburgerMenuImageControl2"]).Begin();
			}
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
			if (rootFrame.CanGoBack && e.Handled == false)
			{
				e.Handled = true;
				rootFrame.GoBack();
				while (rootFrame.CurrentSourcePageType == typeof(Option) && rootFrame.CanGoBack)
				{
					e.Handled = true;
					rootFrame.GoBack();
				}
			}
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
			else if (MyFrame.CurrentSourcePageType == typeof(Manual))
			{
				this.IconListBox.SelectedIndex = 1;
				this.configListBox.SelectedItem = null;
			}
			else if (MyFrame.CurrentSourcePageType == typeof(Feedback))
			{
				this.IconListBox.SelectedIndex = 3;
				this.configListBox.SelectedItem = null;
			}
			else if (MyFrame.CurrentSourcePageType == typeof(AboutPage))
			{
				this.configListBox.SelectedIndex = 0;
				this.IconListBox.SelectedItem = null;
			}
			else if (MyFrame.CurrentSourcePageType == typeof(EastEggPage))
			{
				this.IconListBox.SelectedIndex = 2;
				this.configListBox.SelectedItem = null;
			}
		}
		private void HamburgerButton_Click(object sender, RoutedEventArgs e)
		{
			if (!mySplitView.IsPaneOpen)
				((Storyboard)this.Resources["HamburgerMenuImageControl"]).Begin();
			else
				((Storyboard)this.Resources["HamburgerMenuImageControl2"]).Begin();
			// if (!isWideMode)
			this.mySplitView.IsPaneOpen = !this.mySplitView.IsPaneOpen;

		}

		private void IconListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (HomeListBoxItem.IsSelected)
			{
				if (this.mySplitView.IsPaneOpen && (!isWideMode))
				{
					this.mySplitView.IsPaneOpen = !this.mySplitView.IsPaneOpen;
				}
				if (Config.isSuperWideMode)
				{
					this.sideFrameGrid.MaxWidth = int.MaxValue;
					myFrame2.Navigate(typeof(waitPage));
				}
				configListBox.SelectedItem = null;
				MyFrame.Navigate(typeof(ListPage));
			}
			if (ManualListBoxItem.IsSelected)
			{
				if (this.mySplitView.IsPaneOpen && (!isWideMode))
				{
					this.mySplitView.IsPaneOpen = !this.mySplitView.IsPaneOpen;
				}
				this.sideFrameGrid.MaxWidth = 0;
				configListBox.SelectedItem = null;
				MyFrame.Navigate(typeof(Manual));
			}
			if (FeedbackListBoxItem.IsSelected)
			{
				if (this.mySplitView.IsPaneOpen && (!isWideMode))
				{
					this.mySplitView.IsPaneOpen = !this.mySplitView.IsPaneOpen;
				}
				this.sideFrameGrid.MaxWidth = 0;
				configListBox.SelectedItem = null;
				MyFrame.Navigate(typeof(Feedback));
			}
			if (FeatureTestBoxItem.IsSelected)
			{
				if (this.mySplitView.IsPaneOpen && (!isWideMode))
				{
					this.mySplitView.IsPaneOpen = !this.mySplitView.IsPaneOpen;
				}
				this.sideFrameGrid.MaxWidth = 0;
				configListBox.SelectedItem = null;
				MyFrame.Navigate(typeof(EastEggPage));
			}
		}

		private void configListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (configListBoxItem.IsSelected)
			{
				if (this.mySplitView.IsPaneOpen && (!isWideMode))
				{
					this.mySplitView.IsPaneOpen = !this.mySplitView.IsPaneOpen;
				}
				this.sideFrameGrid.MaxWidth = 0;
				IconListBox.SelectedItem = null;
				MyFrame.Navigate(typeof(AboutPage));
			}
		}

		private void mySplitView_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
		{
			((Storyboard)this.Resources["HamburgerMenuImageControl2"]).Begin();
		}
	}
}


