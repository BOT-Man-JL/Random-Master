using RandomMaster;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Storage;

namespace RandomMasterService
{
	public sealed class RandomMasterBg : IBackgroundTask
	{
		BackgroundTaskDeferral serviceDeferral;
		VoiceCommandServiceConnection serviceConnection;
		ResourceLoader StringLoader;
		ObservableCollection<Ques> queses;
		QuesManager qm;

		public async void Run(IBackgroundTaskInstance taskInstance)
		{
			serviceDeferral = taskInstance.GetDeferral();
			taskInstance.Canceled += OnTaskCanceled;

			var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

			// Is the certain app service
			if (triggerDetails != null && triggerDetails.Name == "RandomMasterService")
			{
				serviceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);
				serviceConnection.VoiceCommandCompleted += OnVoiceCommandCompleted;

				VoiceCommand voiceCommand = await serviceConnection.GetVoiceCommandAsync();
				StringLoader = new ResourceLoader();

				Config.LoadConfig();
				qm = new QuesManager();
				queses = qm.queses;

				// No need to find out which command
				//switch (voiceCommand.CommandName)
				//{
				//case "Service":
				//	if (voiceCommand.Properties.Any(s => s.Key == "Text") &&
				//		voiceCommand.Properties["Text"][0] != null &&
				//		voiceCommand.Properties["Text"][0] != "...")
				//		await Process(voiceCommand.Properties["Text"][0]);
				//	else
				//		await ReErrInput();
				//	break;

				//default:
				//	await ReErrInput();
				//	break;
				//}

				if (voiceCommand.Properties.Any(s => s.Key == "Text") &&
					voiceCommand.Properties["Text"][0] != null &&
					voiceCommand.Properties["Text"][0] != "...")
				{
					if (Config.tutorialProcess != 0)
						await Tutorial(voiceCommand.Properties["Text"][0]);
					else
						await Process(voiceCommand.Properties["Text"][0]);
				}
				else
					await ReErrInput();
			}
		}

		private async Task Tutorial(string inStr)
		{
			var userPrompt = new VoiceCommandUserMessage();
			var isRight = true;
			var count = int.Parse(StringLoader.GetString("TutorialCount"));

			// Match Each Keyword
			foreach (var keyword in StringLoader.GetString
				($"TutorialKeyword{Config.tutorialProcess}").Split(' '))
				isRight = isRight && inStr.Contains(keyword);

			// Enter Tutorial Mode
			if (Config.tutorialProcess == 1)
				isRight = true;

			if (isRight)
			{
				// Push Down Automata
				userPrompt.SpokenMessage =
					StringLoader.GetString($"TutorialSuccess{Config.tutorialProcess}a");
				userPrompt.DisplayMessage =
					StringLoader.GetString($"TutorialSuccess{Config.tutorialProcess}b");

				if (Config.tutorialProcess != count)
					++Config.tutorialProcess;
				else
					Config.tutorialProcess = 0;
				Config.SaveConfig();

				// Specific Action
				ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
				switch (Config.tutorialProcess)
				{
				case 0:
					await serviceConnection.ReportProgressAsync(VoiceCommandResponse.CreateResponse(userPrompt));

					var optList = new List<string>
					{
						(string)localSettings.Values["Tutorial_Opt1"],
						(string)localSettings.Values["Tutorial_Opt2"]
					};
					qm.SetQues((string)localSettings.Values["Tutorial_Ques"], optList, null);
					qm.Save();
					await Task.Delay(5000);

					var response = VoiceCommandResponse.CreateResponse(userPrompt);
					response.AppLaunchArgument = "TutorialDone";
					await serviceConnection.RequestAppLaunchAsync(response);
					break;

				case 5:
					localSettings.Values["Tutorial_Ques"] = inStr;

					await serviceConnection.ReportSuccessAsync(VoiceCommandResponse.CreateResponse(userPrompt));
					break;

				case 6:
					inStr = inStr.Substring(inStr.IndexOf("添加") + 2);
					localSettings.Values["Tutorial_Opt1"] = inStr.Substring(0, inStr.IndexOf("或者"));
					localSettings.Values["Tutorial_Opt2"] = inStr.Substring(inStr.IndexOf("或者") + 2);

					await serviceConnection.ReportSuccessAsync(VoiceCommandResponse.CreateResponse(userPrompt));
					break;

				default:
					await serviceConnection.ReportSuccessAsync(VoiceCommandResponse.CreateResponse(userPrompt));
					break;
				}
			}
			else
			{
				userPrompt.SpokenMessage =
					StringLoader.GetString($"TutorialFailed{Config.tutorialProcess}a");
				userPrompt.DisplayMessage =
					StringLoader.GetString($"TutorialFailed{Config.tutorialProcess}b") + '\n' +
					StringLoader.GetString("TutorialQuitHint");

				await serviceConnection.ReportFailureAsync(VoiceCommandResponse.CreateResponse(userPrompt));
			}
		}

		private async Task Process(string ques)
		{
			var ques_old = ques;
			var rand = new Random(DateTime.Now.Second * DateTime.Now.Millisecond);

			NLP.Word[] words;
			var opts = new List<string>();
			var fOR = false;

			// #0.0 Add Options
			if (ques.Substring(0, 2).Equals("添加"))
			{
				await AddOptions(ques.Substring(2));
				return;
			}

			// #0.1 Easter Egg Mode (1/5 possibility)
			if (Config.isEEon && (rand.Next(0, 5) == 2))
			{
				List<string> anses = new List<string>();

				// For a certain topic
				for (int i = int.Parse(StringLoader.GetString("EE_count")); i > 0; i--)
					foreach (var str in StringLoader.GetString($"EE_q{i}").Split())
						// for each keyword
						if (ques.Contains(str))
						{
							for (int j = int.Parse(StringLoader.GetString($"EE_q{i}_c")); j > 0; j--)
								anses.Add(StringLoader.GetString($"EE_q{i}_a{j}"));

							// Easteregg Achievement Hit
							AccomplishmentsManager.ReportAccomplishment(i + 4);

							// Move to next Easteregg
							break;
						}

				// General answers
				for (int j = int.Parse(StringLoader.GetString("EE_q0_c")); j > 0; j--)
					anses.Add(StringLoader.GetString($"EE_q0_a{j}"));

				await ReByAnses(anses, rand);
				return;
			}

			// #0.2 Prompt Processing
			await ReProcess();

			// #0.3 Polish and Analyze the quesiton
			NLP.PolishQues(ref ques, ref opts, ref fOR);
			try
			{
				words = await NLP.AnalyzeQues(ques);
			}
			catch (Exception e)
			{
				await ReErrInput();
				System.Diagnostics.Debug.WriteLine(e.Message);
				return;
			}

			// #0.5 Get Feature
			var verb = -1;
			foreach (var i in words)
				if (i.pa == -1 && i.re == "HED" && i.pos == "v")
				{
					verb = i.id;
					break;
				}
			if (verb == -1)
				foreach (var i in words)
					if (i.pa == -1 && i.re == "Root" && i.pos == "v")
					{
						verb = i.id;
						break;
					}

			// #0.6 Update Asking count for Achievement
			Config.cAsking++;
			Config.SaveConfig();

			// #1 Yes-OR-No Logic

			// #1.1 Ending with '吗' or Containing '是否'
			if (ques[ques.Length - 1] == '吗')
			{
				List<string> anses = new List<string>();

				if (verb == -1)
					foreach (var i in words)
						if (i.pa == -1 && i.re == "Root")
						{
							verb = i.id;
							break;
						}

				anses.Add($"当然{words[verb].cont}啦~");
				anses.Add($"哼哼，不{words[verb].cont}呢~");

				await ReByAnses(anses, rand);
				return;
			}
			else if (ques.Contains("是否"))
			{
				List<string> anses = new List<string>();

				if (verb == -1)
					foreach (var i in words)
						if (i.pa == -1 && i.re == "Root")
						{
							verb = i.id;
							break;
						}

				anses.Add($"当然{words[verb].cont}的呢~");
				anses.Add($"哼哼，不{words[verb].cont}啦~");

				await ReByAnses(anses, rand);
				return;
			}

			// #1.2 Containing '不'
			if (ques.Contains("不") && ques.Length > 3)
			{
				List<string> anses = new List<string>();
				var fYesOrNo = false;

				foreach (var i in words)
					if (NLP.ContainOrIs(i.cont, "不") && i.id - 1 >= 0)
					{
						// Example: 要 不要
						if (i.cont.Length > 1)
						{
							if (NLP.ContainOrIs(i.cont, words[i.id - 1].cont))
							{
								anses.Add($"还是{i.cont}啦~");
								i.cont.Replace("不", "");
								anses.Add($"那就{i.cont}吧~");
								fYesOrNo = true;
								break;
							}
						}
						// Example: 可 不 可以
						else if (i.id + 1 < words.Count() &&
							NLP.ContainOrIs(words[i.id + 1].cont, words[i.id - 1].cont))
						{
							anses.Add($"哈哈，{words[i.id + 1].cont}吧~");
							anses.Add($"不{words[i.id + 1].cont}哦~");
							fYesOrNo = true;
							break;
						}
					}
				if (fYesOrNo)
				{
					await ReByAnses(anses, rand);
					return;
				}
			}

			// #2 OR Logic

			if (fOR)
			{
				var OrId = -1;
				foreach (var i in words)
					if (i.cont == "或")
					{
						OrId = i.id;
						if (i.semre != "mConj")
						{
							await ReSyntaxErr(ques_old);
							break;
						}
						break;
					}
				if (OrId < 1)
				{
					await ReSyntaxErr(ques_old);
					return;
				}

				var j = words[OrId].sempa;
				if (OrId > j)
				{
					await ReSyntaxErr(ques_old);
					return;
				}

				if (NLP.IsNoun(words[j].pos))
				{
					var str = "";
					for (int i = OrId + 1; i <= j; i++)
						str += words[i].cont;
					opts.Add(str);

					int ModId = -1;
					if (OrId >= 2)
						for (var i = OrId - 2; i >= 0; i--)
							if (words[i].sempa == OrId - 1)
								ModId = i;
					if (ModId != -1)
						for (str = ""; ModId < OrId; ModId++)
							str += words[ModId].cont;
					else
						str = words[OrId - 1].cont;
					opts.Add(str);
				}
				else if (words[j].pos == "v")
				{
					var str = "";
					int NId = j;
					if (j + 1 < words.Count())
					{
						for (var i = j + 1; i < words.Count(); i++)
							if (words[i].sempa == j)
								switch (words[i].semre)
								{
								case "Pat":
								case "Cont":
								case "Dir":
								case "Prod":
								case "Bleg":
								case "Tool":
									NId = i;
									break;
								}

						// find syntax's VOB if no object
						if (NId == j)
							for (var i = j + 1; i < words.Count(); i++)
								if (words[i].pa == j && words[i].re == "VOB")
									NId = i;
					}
					for (int i = OrId + 1; i <= NId; i++)
						str += words[i].cont;
					opts.Add(str);

					str = "";
					int VId;
					for (VId = OrId - 1; VId >= 0; VId--)
						if (words[VId].pos == "v")
							break;
					if (VId != -1)
						for (; VId < OrId; VId++)
							str += words[VId].cont;
					opts.Add(str);
				}
				else
				{
					await ReSyntaxErr(ques_old);
					return;
				}

				await ReByOpts(opts, rand);
				return;
			}

			// #3 User Defined

			if (verb == -1)
			{
				await ReSyntaxErr(ques_old);
				return;
			}

			var feature = NLP.GetFeature(words);
			var bestQues = NLP.BestMatch(queses, feature, ques);

			if (bestQues != null && bestQues.options.Count != 0)
			{
				opts = bestQues.options.ToList();
				await ReByOpts(opts, rand);
				return;
			}
			else if (bestQues != null)
			{
				// No Option for the Existing Question
				await ReAddOpts(bestQues.question, bestQues.feature);
				return;
			}
			else
			{
				// No Existing Question
				await ReAddOpts(ques_old, feature);
				return;
			}
		}

		private async Task AddOptions(string text)
		{
			var userPrompt = new VoiceCommandUserMessage();

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
			ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
			ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();

			var ques = (string)localSettings.Values["CurrentQues"];
			if (ques == null)
			{
				userPrompt.SpokenMessage = StringLoader.GetString("Prompt_NoQues1");
				userPrompt.DisplayMessage = StringLoader.GetString("Prompt_NoQues2");

				await serviceConnection.ReportFailureAsync(VoiceCommandResponse.CreateResponse(userPrompt));
			}
			else
			{
				List<string> opts = new List<string>();

				// Polish row text
				text = text.Replace("还是", "或");
				text = text.Replace("或者", "或");
				text = text.Replace("或是", "或");

				// Extract options
				var pos = text.IndexOf('或');
				while (pos != -1)
				{
					if (pos != 0)
						opts.Add(text.Substring(0, pos));

					if (pos != text.Length - 1)
						text = text.Substring(pos + 1);
					else
						break;

					pos = text.IndexOf('或');
				}

				// Insert final option
				if (text != "或")
					opts.Add(text);

				// Get ...
				if (opts.Any(s => s == "..."))
				{
					await ReErrInput();
					return;
				}

				// Confirm to Add
				var userReprompt = new VoiceCommandUserMessage();

				userPrompt.SpokenMessage = StringLoader.GetString("Prompt_AddToQues1");
				userPrompt.DisplayMessage = StringLoader.GetString("Prompt_AddToQues2") + $" \"{ques}\"";

				userReprompt.SpokenMessage = StringLoader.GetString("Prompt_AddToQues3");
				userReprompt.DisplayMessage = StringLoader.GetString("Prompt_AddToQues4") + $" \"{ques}\"";

				var response = VoiceCommandResponse.CreateResponseForPrompt(userPrompt, userReprompt);
				await serviceConnection.RequestConfirmationAsync(response);

				// Extract feature of the question
				composite = (ApplicationDataCompositeValue)localSettings.Values["CurrentFeat"];
				if (composite != null)
					qm.SetQues(ques, opts, (string[])composite["Feat"]);

				// Impossible to happen
				//else
				//	qm.SetQues(ques, opts, null);

				// Save Questions
				qm.Save();
				ApplicationData.Current.SignalDataChanged();

				// Clear Temp Setting
				localSettings.Values["CurrentQues"] = null;
				localSettings.Values["CurrentFeat"] = null;

				// Response
				userPrompt.SpokenMessage = StringLoader.GetString("Prompt_SetQues");
				userPrompt.DisplayMessage = StringLoader.GetString("Prompt_SetQuesPre")
					+ ques + StringLoader.GetString("Prompt_SetQuesPost");

				await serviceConnection.ReportSuccessAsync(VoiceCommandResponse.CreateResponse(userPrompt));
			}
		}

		private async Task ReByAnses(List<string> anses, Random rand)
		{
			var userPrompt = new VoiceCommandUserMessage();

			rand.Next(rand.Next(rand.Next(anses.Count)));
			userPrompt.DisplayMessage = userPrompt.SpokenMessage = anses[rand.Next(0, anses.Count)];

			//if (Config.isFBon)
			//	await ReAskForFeedback(userPrompt, null);
			//else

			await serviceConnection.ReportSuccessAsync(VoiceCommandResponse.CreateResponse(userPrompt));
		}

		private async Task ReByOpts(List<string> opts, Random rand)
		{
			var userPrompt = new VoiceCommandUserMessage();

			rand.Next(DateTime.Now.Millisecond);
			var ans_tmp = opts[rand.Next(0, opts.Count)];

			rand.Next(DateTime.Now.Millisecond);
			var index = rand.Next(0, int.Parse(StringLoader.GetString("cPattern"))) + 1;
			userPrompt.DisplayMessage = userPrompt.SpokenMessage =
				StringLoader.GetString($"Prefix{index}") +
				$" {ans_tmp} " +
				StringLoader.GetString($"Postfix{index}");

			//if (Config.isFBon)
			//	await ReAskForFeedback(userPrompt, ans_tmp);
			//else

			await serviceConnection.ReportSuccessAsync(VoiceCommandResponse.CreateResponse(userPrompt));
		}

		//private async Task ReAskForFeedback(VoiceCommandUserMessage userPrompt, string keyword)
		//{
		//	var askForFeedback1 = StringLoader.GetString("Prompt_AskForFeedback1");
		//	var askForFeedback2 = StringLoader.GetString("Prompt_AskForFeedback2");
		//	var askForFeedback3 = StringLoader.GetString("Prompt_AskForFeedback3");

		//	userPrompt.SpokenMessage += ";" + askForFeedback1;
		//	userPrompt.DisplayMessage += "\n" + askForFeedback2;
		//	userPrompt.DisplayMessage += "\n" + askForFeedback3;

		//	var userReprompt = new VoiceCommandUserMessage();
		//	userReprompt.SpokenMessage = askForFeedback1;
		//	userReprompt.DisplayMessage = askForFeedback2;

		//	var result = await serviceConnection.RequestConfirmationAsync
		//		(VoiceCommandResponse.CreateResponseForPrompt(userPrompt, userReprompt));

		//	if (result != null)  // User Cancel => result = null
		//		if (result.Confirmed)
		//		{
		//			if (Config.isRCon && keyword != null)
		//			{
		//				userPrompt.SpokenMessage = StringLoader.GetString("Prompt_ProcessRec1") + keyword;
		//				userPrompt.DisplayMessage = StringLoader.GetString("Prompt_ProcessRec2Pre") +
		//					$" {keyword} " + StringLoader.GetString("Prompt_ProcessRec2Post");
		//				await serviceConnection.ReportProgressAsync(VoiceCommandResponse.CreateResponse(userPrompt));

		//				await ReRecommend(keyword);
		//			}
		//			else
		//			{
		//				userPrompt.SpokenMessage = StringLoader.GetString("Prompt_GoodFeedback1");
		//				userPrompt.DisplayMessage = StringLoader.GetString("Prompt_GoodFeedback2");

		//				await serviceConnection.ReportSuccessAsync(VoiceCommandResponse.CreateResponse(userPrompt));
		//			}
		//		}
		//		else
		//		{
		//			userPrompt.SpokenMessage = StringLoader.GetString("Prompt_BadFeedback1");
		//			userPrompt.DisplayMessage = StringLoader.GetString("Prompt_BadFeedback2");

		//			await serviceConnection.ReportFailureAsync(VoiceCommandResponse.CreateResponse(userPrompt));
		//		}
		//}

		//private async Task ReRecommend(string keyword)
		//{
		//	var dianping = new ParseDianPing();
		//	var result = await dianping.GetOne(keyword);

		//	var userPrompt = new VoiceCommandUserMessage();
		//	userPrompt.SpokenMessage = result.name + StringLoader.GetString("Prompt_Recommend1");
		//	userPrompt.DisplayMessage = StringLoader.GetString("Prompt_Recommend2") + " " + result.name;

		//	var tile = new VoiceCommandContentTile();
		//	tile.Title = result.name;
		//	tile.TextLine1 = result.address;
		//	tile.TextLine3 = StringLoader.GetString("Prompt_Recommend4");
		//	tile.AppLaunchArgument = result.link;
		//	tile.ContentTileType = VoiceCommandContentTileType.TitleWithText;

		//	await serviceConnection.ReportSuccessAsync(
		//		VoiceCommandResponse.CreateResponse(
		//			userPrompt, new List<VoiceCommandContentTile> { tile }));
		//}

		private async Task ReAddOpts(string ques, string[] feature)
		{
			var userPrompt = new VoiceCommandUserMessage();

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
			ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();

			userPrompt.SpokenMessage = StringLoader.GetString("Prompt_AddOpt1");
			userPrompt.DisplayMessage = StringLoader.GetString("Prompt_AddOpt2");

			localSettings.Values["CurrentQues"] = ques;
			composite["Feat"] = feature;
			localSettings.Values["CurrentFeat"] = composite;

			await serviceConnection.ReportFailureAsync(VoiceCommandResponse.CreateResponse(userPrompt));
		}

		private async Task ReProcess()
		{
			var userPrompt = new VoiceCommandUserMessage();

			userPrompt.SpokenMessage = StringLoader.GetString("Prompt_Processing1");
			userPrompt.DisplayMessage = StringLoader.GetString("Prompt_Processing2");

			await serviceConnection.ReportProgressAsync(VoiceCommandResponse.CreateResponse(userPrompt));
		}

		private async Task ReSyntaxErr(string ques_old)
		{
			var userPrompt = new VoiceCommandUserMessage();

			userPrompt.SpokenMessage = StringLoader.GetString("Prompt_ErrSyntax");
			userPrompt.DisplayMessage = StringLoader.GetString("Prompt_ErrSyntaxPre")
				+ $" \"{ques_old}\" "
				+ StringLoader.GetString("Prompt_ErrSyntaxPost");

			await serviceConnection.ReportFailureAsync(VoiceCommandResponse.CreateResponse(userPrompt));
		}

		private async Task ReErrInput()
		{
			var userPrompt = new VoiceCommandUserMessage();

			userPrompt.SpokenMessage = StringLoader.GetString("Prompt_ErrInput1");
			userPrompt.DisplayMessage = StringLoader.GetString("Prompt_ErrInput2");

			await serviceConnection.ReportFailureAsync(VoiceCommandResponse.CreateResponse(userPrompt));
		}

		private void OnVoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
		{
			if (this.serviceDeferral != null)
			{
				this.serviceDeferral.Complete();
			}
		}

		private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
		{
			System.Diagnostics.Debug.WriteLine("Task cancelled, clean up");
			if (serviceDeferral != null)
			{
				//Complete the service deferral
				serviceDeferral.Complete();
			}
		}
	}
}
