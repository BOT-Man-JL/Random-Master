using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Storage;

namespace RandomMaster
{
	public class NotificationPoint : BindableBase
	{
		public float Opacity = 0;
		public float opacity
		{
			get { return Opacity; }
			set { SetProperty(ref Opacity, value); }
		}
	}

	public class Config
	{
		static public bool isSuperWideMode = false;
		static public bool isFirstTime = true;
		static public int tutorialProcess = 0;
		static public bool isVcdInstalled = false;

		// Roaming
		static public bool isEEon = false;
		//static public bool isFBon = false;
		//static public bool isRCon = false;
		static public int cAsking = 0;

		// Achievement related
		static public List<int> accomplishments_records = new List<int> { };
		static public List<int> accomplishments_unnotified = new List<int> { };
		static public NotificationPoint redpoint = new NotificationPoint();

		static private ApplicationDataContainer localSettings =
					ApplicationData.Current.LocalSettings;
		static private ApplicationDataContainer roamingSettings =
			ApplicationData.Current.RoamingSettings;

		static public void SaveConfig()
		{
			// #1 Local Settings
			ApplicationDataCompositeValue composite =
				new ApplicationDataCompositeValue();

			composite["isFirstTime"] = isFirstTime;
			composite["tutorialProcess"] = tutorialProcess;
			composite["isVcdInstalled"] = isVcdInstalled;


			localSettings.Values["Configurations"] = composite;

			// #2 Roaming Settings
			composite = new ApplicationDataCompositeValue();

			composite["isEEon"] = isEEon;
			//composite["isFBon"] = isFBon;
			//composite["isRCon"] = isRCon;
			composite["cAsking"] = cAsking;

            string str = "";
			foreach (var item in accomplishments_records.Distinct())
				str += (item + " ");
			composite["accomplishments_records"] = str;

			str = "";
			foreach (var item in accomplishments_unnotified.Distinct())
				str += (item + " ");
			composite["accomplishments_unnotified_string"] = str;

			roamingSettings.Values["Configurations"] = composite;
		}

		static public void LoadConfig()
		{
			// #1 Local Settings
			ApplicationDataCompositeValue composite =
				(ApplicationDataCompositeValue)localSettings.Values["Configurations"];

			if (composite != null)
			{
				if (composite["isFirstTime"] != null)
					isFirstTime = (bool)composite["isFirstTime"];

				if (composite["tutorialProcess"] != null)
					tutorialProcess = (int)composite["tutorialProcess"];

				if (composite["isVcdInstalled"] != null)
					isVcdInstalled = (bool)composite["isVcdInstalled"];
			}

			// #2 Roaming Settings
			composite =
				(ApplicationDataCompositeValue)roamingSettings.Values["Configurations"];
			if (composite != null)
			{
				if (composite["isEEon"] != null)
					isEEon = (bool)composite["isEEon"];
				//if (composite["isFBon"] != null)
				//	isFBon = (bool)composite["isFBon"];
                //if (composite["isRCon"] != null)
                //    isRCon = (bool)composite["isRCon"];
				if (composite["cAsking"] != null)
					cAsking = (int)composite["cAsking"];

				if (composite["accomplishments_records"] != null)
				{
					var str = (string)composite["accomplishments_records"];
					foreach (var item in str.Split())
					{
						int result;
						if (int.TryParse(item, out result) && !accomplishments_records.Contains(result))
							accomplishments_records.Add(result);
					}
				}

				if (composite["accomplishments_unnotified_string"] != null)
				{
					var str = (string)composite["accomplishments_unnotified_string"];
					foreach (var item in str.Split())
					{
						int result;
						if (int.TryParse(item, out result))
							AccomplishmentsManager.ReportAccomplishment(result);
					}
				}

				// Achievement #3 asking-10
				if (cAsking >= 10)
					AccomplishmentsManager.ReportAccomplishment(3);
			}
		}

		static public async Task<bool> InstallVcdFile()
		{
			try
			{
				var file = await StorageFile.GetFileFromApplicationUriAsync
					(new Uri("ms-appx:///VoiceCommandSet.xml"));

				// Since there's no simple way to test that the VCD has been imported,
				// or that it's your most recent version, it's not unreasonable to do this upon app load.
				await
					Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.
					InstallCommandDefinitionsFromStorageFileAsync(file);

				return true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Installing Voice Commands Failed: " + ex.ToString());
				return false;
			}
		}

		public static bool IsInternet()
		{
			ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
			var result = (connections != null) &&
				(connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
			return result;
		}
	}
}
