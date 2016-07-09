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
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Resources;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace RandomMaster
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	/// 
	public class accomplishment : BindableBase
	{
		private int no { get; set; }
		private string name { get; set; }
		private string description { get; set; }
		public bool isGet { get; set; }

		public accomplishment(int number, string _name, string _description, bool _isGet)
		{
			name = _name;
			no = number;
			isGet = _isGet;
			description = _description;
		}

		public string Name
		{
			get { return (this.isGet) ? name : "???"; }
		}

		public string No
		{
			get { return no.ToString("D2"); }
		}

		public string Description
		{
			get { return (this.isGet) ? description : "???"; }
		}
	}

	public class accomplishments
	{
		public ObservableCollection<accomplishment> all_accomplishments = new ObservableCollection<accomplishment>();

		private int accomplishements_number;
		private List<int> doneRecords;

		public accomplishments()
		{
			var resourceLoader = ResourceLoader.GetForCurrentView("accomplishments");

			accomplishements_number = int.Parse(resourceLoader.GetString("ac-count"));
			doneRecords = Config.accomplishments_records;

			for (int i = 0; i < accomplishements_number; i++)
			{
				var name = resourceLoader.GetString("ac-" + i);
				var desc = resourceLoader.GetString("des-" + i);

				all_accomplishments.Add(new accomplishment(i, name, desc,
					(doneRecords != null) ? doneRecords.Contains(i) : false));
			}
		}

		public ObservableCollection<accomplishment> getAccomplishments()
		{
			return all_accomplishments;
		}
	}


	public class ColorTemplateSelector : DataTemplateSelector
	{
		public DataTemplate DoneItem { get; set; }
		public DataTemplate UndoneItem { get; set; }

		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			accomplishment listData = item as accomplishment;

			return listData.isGet ? DoneItem : UndoneItem;
		}
	}

	public sealed partial class EastEggPage : Page
	{
		public accomplishments _accomplishments;

		public EastEggPage()
		{
			this.InitializeComponent();
			//listView.ItemsSource = new System.Collections.Generic.List<System.String>
			//{ "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
			_accomplishments = new accomplishments();
			if (Config.accomplishments_unnotified.Count != 0)
				popNotification();
		}

		public async void popNotification()
		{
			List<int> accomplishments_unnotified_temp = new List<int>(Config.accomplishments_unnotified);
			foreach (var item in accomplishments_unnotified_temp)
			{
				AccomplishmentDialog accomplishmentsDialog = new AccomplishmentDialog(item);
				await accomplishmentsDialog.ShowAsync();
				AccomplishmentsManager.ProcessAccomplishment(item);
			}
			Frame.Navigate(typeof(EastEggPage));
			Frame.GoBack();
		}

		private void listView_ContainerContentChanging(Windows.UI.Xaml.Controls.ListViewBase sender,
			Windows.UI.Xaml.Controls.ContainerContentChangingEventArgs args)
		{
			args.ItemContainer.Background =
				(Windows.UI.Xaml.Media.SolidColorBrush)sender.Resources["Item" + (args.ItemIndex % 2).ToString()];
		}
	}
}

