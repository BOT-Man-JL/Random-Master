using System;
using System.Linq;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace RandomMaster
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class Option : Page
	{
		public Ques thisQues;
		private TypedEventHandler<ApplicationData, object> dataChangedHandler = null;

		public Option()
		{
			this.InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			thisQues = (Ques)e.Parameter;

			this.options.ItemsSource = thisQues.options;
			this.QuestionBlock.Text = thisQues.question;

			if (thisQues.options.Count() == 0)
			{
				errorText2.Visibility = Visibility.Visible;
				options.Visibility = Visibility.Collapsed;
			}
			else
			{
				errorText2.Visibility = Visibility.Collapsed;
				options.Visibility = Visibility.Visible;
			}

			dataChangedHandler = new TypedEventHandler<ApplicationData, object>(DataChangedHandler);
			ApplicationData.Current.DataChanged += dataChangedHandler;

			base.OnNavigatedTo(e);
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

				if (Frame.CanGoBack)
				{
					Frame.GoBack();
					Frame.Navigate(typeof(Option), App.qm.queses.First(p => p.question == thisQues.question));
				}
				else
				{
					Frame.Navigate (typeof(Option), App.qm.queses.First(p => p.question == thisQues.question));
					Frame.BackStack.Clear();
				}
			});
		}

		private async void AddOptionBtn_Click(object sender, RoutedEventArgs e)
		{
			AddOptionDialogue addOptionWindow = new AddOptionDialogue(thisQues);
			await addOptionWindow.ShowAsync();
			if (thisQues.options.Count() != 0)
			{
				errorText2.Visibility = Visibility.Collapsed;
				options.Visibility = Visibility.Visible;
			}
		}

		private void deleteBtn_Click(object sender, RoutedEventArgs e)
		{
			var list = options.SelectedItems.ToList();
			options.SelectedItems.Clear();

			var opts = thisQues.options;
			foreach (var sel in list)
				opts.Remove((string)sel);
			thisQues.options = opts;

			App.qm.Save();
			if (thisQues.options.Count == 0)
			{
				errorText2.Visibility = Visibility.Visible;
				options.Visibility = Visibility.Collapsed;
			}
		}

		//private void options_ItemClick(object sender, ItemClickEventArgs e)
		//{
		//    this.DeleteBtn.IsEnabled = true;
		//    selOpt = ((ListView)sender).SelectedItem.ToString();
		//}

		private void options_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (options.SelectedItem != null)
			{ this.DeleteBtn.IsEnabled = true; }
			else
			{ this.DeleteBtn.IsEnabled = false; }
		}

		private async void titleModify_Click(object sender, RoutedEventArgs e)
		{
			changeQuestionDialogue changeQuestionWindow = new changeQuestionDialogue(thisQues);
			await changeQuestionWindow.ShowAsync();
		}
	}
}
