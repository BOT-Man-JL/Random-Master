using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace RandomMaster
{
	sealed public class Ques : BindableBase
	{
		private string _question;
		private ObservableCollection<string> _options;
		private string[] _feature;
		private string _preview;

		public Ques(string ques, ObservableCollection<string> opts, string[] feats)
		{
			question = ques;
			options = opts;
			feature = feats;

			_UpdatePreview();
		}

		public string question
		{
			get { return _question; }
			set { SetProperty(ref _question, value); }
		}

		public ObservableCollection<string> options
		{
			get { return _options; }
			set
			{
				SetProperty(ref _options, value);
				_UpdatePreview();
			}
		}

		public string[] feature
		{
			get { return _feature; }
			set
			{
				SetProperty(ref _feature, value);
				_UpdatePreview();
			}
		}

		public string preview
		{
			get { return _preview; }
			set { SetProperty(ref _preview, value); }
		}

		private void _UpdatePreview()
		{
			var str = "";
			if (feature == null)
			{
				var StringLoader = new Windows.ApplicationModel.Resources.ResourceLoader();
				str = StringLoader.GetString("App_QuesNoFeature") + " - ";
			}
			else if (feature[0] == "null")
			{
				var StringLoader = new Windows.ApplicationModel.Resources.ResourceLoader();
				str = StringLoader.GetString("App_QuesBadFeature") + " - ";
			}

			if (options.Count == 0)
			{
				var StringLoader = new Windows.ApplicationModel.Resources.ResourceLoader();
				str += StringLoader.GetString("App_NoOptions");
			}
			else
			{
				if (options.Count() > 3)
				{
					for (int i = 0; i < 3; i++)
					{
						str += options[i] + " ";
					}
					str += "...";
				}
				else
				{
					for (int i = 0; i < options.Count(); i++)
					{
						str += options[i] + " ";
					}
				}
			}

			preview = str;
		}
	}

	public class QuesManager
	{
		private ApplicationDataContainer roamingSettings =
			ApplicationData.Current.RoamingSettings;

		// Remark: Is there need to trace ref count?
		private static ReaderWriterLockSlim lck =
			new ReaderWriterLockSlim();

		public ObservableCollection<Ques> queses;

		public QuesManager()
		{
			queses = new ObservableCollection<Ques>();
			Load();
		}

		public void Load()
		{
			lck.EnterReadLock();

			ApplicationDataCompositeValue composite =
				(ApplicationDataCompositeValue)roamingSettings.Values["Questions"];

			queses.Clear();
			try
			{
				if (composite != null)
					for (int i = 0; i < (int)composite["count"]; i++)
					{
						ObservableCollection<string> opts;

						if (composite[$"Ques{i}_opts"] != null)
							opts = new ObservableCollection<string>((string[])composite[$"Ques{i}_opts"]);
						else
							opts = new ObservableCollection<string>();

						queses.Add(new Ques(
							(string)composite[$"Ques{i}_ques"],
							opts,
							(string[])composite[$"Ques{i}_feat"]
							));
					}
			}
			catch
			{
				// Solve Bad Data Failure
				queses = new ObservableCollection<Ques>();
			}

			lck.ExitReadLock();
		}

		public void Save()
		{
			lck.EnterWriteLock();

			var composite = new ApplicationDataCompositeValue();

			composite["count"] = queses.Count;
			for (int i = 0; i < queses.Count; i++)
			{
				composite[$"Ques{i}_ques"] = queses[i].question;

				if (queses[i].options.Count != 0)
					composite[$"Ques{i}_opts"] = queses[i].options.ToArray();
				else
					composite[$"Ques{i}_opts"] = null;

				composite[$"Ques{i}_feat"] = queses[i].feature;
			}

			roamingSettings.Values["Questions"] = composite;

			// Achievement #2 set question
			if (queses.Count >= 10)
				AccomplishmentsManager.ReportAccomplishment(2);

			lck.ExitWriteLock();
		}

		public void SetQues(string ques, List<string> opts, string[] feature)
		{
			if (queses.Any(p => p.question == ques))
			{
				// Find First Match
				var question = queses.Where(p => p.question == ques).First();

				// Add New Options
				var options = question.options;
				foreach (string option in opts)
					if (!options.Any(s => s == option))
						options.Add(option);
				question.options = options;

				// Change Existing Question's Feature
				if (feature != null)
					question.feature = feature;
			}
			else
				// Add new entry
				queses.Add(new Ques(ques, new ObservableCollection<string>(opts), feature));
		}

		public async Task PeekFeature()
		{
			if (!Config.IsInternet())
				return;

			List<Task<bool>> tasks = new List<Task<bool>>();
			bool fChanged = false;

			foreach (var q in queses)
				if (q.feature == null)
				{
					var t = PeekFeature(q);
					tasks.Add(t);
				}

			// Using concurrency to reduce time cost
			foreach (var f in await Task.WhenAll(tasks))
				fChanged = f || fChanged;

			if (fChanged)
				Save();
		}

		public async Task<bool> PeekFeature(Ques q)
		{
			var ques = q.question;
			NLP.PolishQues(ref ques);
			string[] feat;

			try
			{
				var words = await NLP.AnalyzeQues(ques);
				feat = NLP.GetFeature(words);

				// Remark: Simulate Latency
				//await Task.Delay(5000);
			}
			catch (Exception e)
			{
				// Http Exception
				System.Diagnostics.Debug.WriteLine(e.Message);
				return false;
			}

			q.feature = feat;
			return true;
		}
	}

	//public class QuesesCollection<T> : ObservableCollection<T>
	//{
	//	public void refresh()
	//	{
	//		NotifyCollectionChangedEventArgs e =
	//			new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
	//		this.OnCollectionChanged(e);
	//	}
	//}
}
