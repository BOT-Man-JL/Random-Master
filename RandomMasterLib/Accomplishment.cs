using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace RandomMaster
{
	public class AccomplishmentsManager
	{
		// Process Accomplishment
		public static void ProcessAccomplishment(int category)
		{
			Config.accomplishments_unnotified.Remove(category);

			if (!Config.accomplishments_records.Contains(category))
				Config.accomplishments_records.Add(category);

			// Achievement #4 all-done
			if (Config.accomplishments_records.Count + 1 ==
				int.Parse(Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView
				("accomplishments").GetString("ac-count")))
			{
				ReportAccomplishment(4);
				return;
			}

			if (Config.accomplishments_unnotified.Count == 0)
				Config.redpoint.opacity = 0;

			Config.SaveConfig();
		}

		// Report Accomplishment
		public static void ReportAccomplishment(int category)
		{
			if (!Config.accomplishments_records.Contains(category) &&
				!Config.accomplishments_unnotified.Contains(category))
				Config.accomplishments_unnotified.Add(category);

			if (Config.accomplishments_unnotified.Count != 0)
				Config.redpoint.opacity = 1;

			Config.SaveConfig();
		}
	}
}
