using System;
using System.Linq;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace RandomMaster
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	/// 

	public sealed partial class ListPage : Page
	{
		private TypedEventHandler<ApplicationData, object> dataChangedHandler = null;

		public ListPage()
		{
			this.InitializeComponent();

			// Create the binding description.
			Binding b = new Binding();
			b.Mode = BindingMode.OneWay;
			b.Source = App.qm.queses;

			// Attach the binding to the target.
			questionView.SetBinding(ListView.ItemsSourceProperty, b);

			if (App.qm.queses.Count == 0)
			{
				questionView.Visibility = Visibility.Collapsed;
				this.errorText.Visibility = Visibility.Visible;
			}
			else
			{
				questionView.Visibility = Visibility.Visible;
				errorText.Visibility = Visibility.Collapsed;
			}

			refreshBtn.Visibility = Visibility.Collapsed;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			dataChangedHandler = new TypedEventHandler<ApplicationData, object>(DataChangedHandler);
			ApplicationData.Current.DataChanged += dataChangedHandler;

			base.OnNavigatedTo(e);

			// Remark: Let UI thread to PeekFeature, and Wait a short time
			App.qm.PeekFeature().Wait(500);
		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			ApplicationData.Current.DataChanged -= dataChangedHandler;
			dataChangedHandler = null;

			base.OnNavigatingFrom(e);
		}

		async void DataChangedHandler(ApplicationData appData, object o)
		{
			// DataChangeHandler may be invoked on a background thread
			// so use the Dispatcher to invoke the UI-related code on the UI thread. 
			await this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
			{
				Config.LoadConfig();
				App.qm.Load();
			});
		}

		private DependencyObject getGrid(DependencyObject e)
		{
			DependencyObject result;
			result = VisualTreeHelper.GetParent(e);
			Type name = result.GetType();
			if (name != typeof(Grid))
				result = getGrid(result);
			return result;
		}

		private void ListView_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (questionView.SelectionMode == ListViewSelectionMode.Single)
			{
				var thisQues = (Ques)e.ClickedItem;

				if (!Config.isSuperWideMode)
					Frame.Navigate(typeof(Option), thisQues);
				else
					((Frame)findRightFrame(this)).Navigate(typeof(Option), thisQues);
			}
		}

		private DependencyObject findRightFrame(DependencyObject e)
		{
			Grid targetGrid;
			Border targetBorder;
			Frame targetFrame;
			targetGrid = (Grid)getGrid(e);
			targetBorder = (Border)VisualTreeHelper.GetChild(targetGrid, 1);
			targetFrame = (Frame)VisualTreeHelper.GetChild(targetBorder, 0);
			return targetFrame;
		}

		private async void addQuesBut_Click(object sender, RoutedEventArgs e)
		{
			AddQuesDialogue addnewques = new AddQuesDialogue("");
			await addnewques.ShowAsync();
			if (App.qm.queses.Count == 0)
			{
				questionView.Visibility = Visibility.Collapsed;
				this.errorText.Visibility = Visibility.Visible;
			}
			else
			{
				questionView.Visibility = Visibility.Visible;
				errorText.Visibility = Visibility.Collapsed;
			}
		}

		private async void deleteQuesBtn_Click(object sender, RoutedEventArgs e)
		{
			ensureDialogue ensureWindow = new ensureDialogue();
			await ensureWindow.ShowAsync();

			if (ensureWindow.result)
			{
				var list = questionView.SelectedItems.ToList();
				questionView.SelectedItems.Clear();

				foreach (var sel in list)
					App.qm.queses.Remove((Ques)sel);

				App.qm.Save();
				if (App.qm.queses.Count == 0)
				{
					questionView.Visibility = Visibility.Collapsed;
					errorText.Visibility = Visibility.Visible;
					leaveEditMode_Click(this.leaveEditMode, null);
				}
				((Frame)findRightFrame(this)).Navigate(typeof(waitPage));
			}
		}

		private void enterEditMode_Click(object sender, RoutedEventArgs e)
		{
			addQuesBut.Visibility = Visibility.Collapsed;
			deleteQuesBtn.Visibility = Visibility.Visible;
			deleteQuesBtn.IsEnabled = false;
			//refreshBtn.Visibility = Visibility.Collapsed;

			enterEditMode.Visibility = Visibility.Collapsed;
			leaveEditMode.Visibility = Visibility.Visible;

			questionView.SelectionMode = ListViewSelectionMode.Multiple;
			//questionView.IsItemClickEnabled = false;
		}

		private void leaveEditMode_Click(object sender, RoutedEventArgs e)
		{
			addQuesBut.Visibility = Visibility.Visible;
			deleteQuesBtn.Visibility = Visibility.Collapsed;
			//refreshBtn.Visibility = Visibility.Visible;

			enterEditMode.Visibility = Visibility.Visible;
			leaveEditMode.Visibility = Visibility.Collapsed;

			questionView.SelectionMode = ListViewSelectionMode.Single;
			questionView.IsItemClickEnabled = true;
			questionView.SelectedItem = null;
		}

		private void questionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (questionView.SelectedItem != null)
				deleteQuesBtn.IsEnabled = true;
			else
				deleteQuesBtn.IsEnabled = false;
		}


		private void refreshBtn_Click(object sender, RoutedEventArgs e)
		{
			App.qm.Load();
		}
	}
}
