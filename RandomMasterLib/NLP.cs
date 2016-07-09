using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RandomMaster
{
	public class NLP
	{
		public class Word
		{
			[JsonProperty("id")]
			public int id { get; set; }

			[JsonProperty("cont")]
			public string cont { get; set; }

			[JsonProperty("pos")]
			public string pos { get; set; }

			[JsonProperty("semparent")]
			public int sempa { get; set; }

			[JsonProperty("semrelate")]
			public string semre { get; set; }

			[JsonProperty("parent")]
			public int pa { get; set; }

			[JsonProperty("relate")]
			public string re { get; set; }
		}

		// Helper Function
		public static bool ContainOrIs(string src, string obj)
		{
			return src.Contains(obj) || src.Equals(obj);
		}

		public static bool IsNoun(string pos)
		{
			return
				pos == "i" ||       // idiom
				pos == "j" ||       // abbreviation
				pos == "k" ||       // suffix
				pos == "m" ||       // number
				pos == "n" ||       // general noun
				pos == "nh" ||      // direciton
				pos == "nd" ||      // person
				pos == "ni" ||      // organization
				pos == "nl" ||      // location
				pos == "ns" ||      // geographic
				pos == "nt" ||      // temporal
				pos == "nz" ||      // other noun
				pos == "r";          // pronoun
		}

		private static bool IsWhat(string str)
		{
			return ContainOrIs(str, "什么") ||
				ContainOrIs(str, "哪") ||
				ContainOrIs(str, "啥") ||
				ContainOrIs(str, "谁") ||
				ContainOrIs(str, "多少") ||
				ContainOrIs(str, "几");
		}

		private static bool IsHow(string str)
		{
			return ContainOrIs(str, "如何") ||
				ContainOrIs(str, "怎");
		}

		private static bool IsWhen(string str)
		{
			return ContainOrIs(str, "时候");
		}

		private static bool IsWhere(string str)
		{
			return ContainOrIs(str, "地方") ||
				ContainOrIs(str, "哪");
		}

		private static bool IsGo(string str)
		{
			return ContainOrIs(str, "去") ||
				ContainOrIs(str, "来") ||
				ContainOrIs(str, "到") ||
				ContainOrIs(str, "上");
		}

		// To improve the analysis
		// #v1 simple version
		public static void PolishQues(ref string ques)
		{
			// Erase bad chars
			ques = ques.Replace(" ", "");
			ques = ques.Replace("，", "");
			ques = ques.Replace("。", "");
			ques = ques.Replace("？", "");

			//// Polish hell pronouns
			ques = ques.Replace("啥", "什么");
			ques = ques.Replace("神马", "什么");

			// Polish OR logic
			if (ques.Contains("或") ||
				ques.Contains("还是") ||
				ques.Contains("要么"))
			{
				//fOrStatement = true;
				ques = ques.Replace("还是", "或");
				ques = ques.Replace("或者", "或");
				ques = ques.Replace("或是", "或");
				ques = ques.Replace("要么", "或");

				// Cut the substring between two '或'
				var pos1 = ques.IndexOf('或');
				var pos2 = ques.IndexOf('或', pos1 + 1);
				while (pos2 != -1)
				{
					//opts.Add(ques.Substring(pos1 + 1, pos2 - pos1 - 1));
					if (pos1 >= 1)
						ques = ques.Substring(0, pos1 - 1) + ques.Substring(pos2);
					else
						ques = ques.Substring(1);

					pos1 = ques.IndexOf('或');
					pos2 = ques.IndexOf('或', pos1 + 1);
				}
			}
		}

		// To improve the analysis
		// #v2 with OR statement
		public static void PolishQues(ref string ques, ref List<string> opts, ref bool fOR)
		{
			// Erase bad chars
			ques = ques.Replace(" ", "");
			ques = ques.Replace("，", "");
			ques = ques.Replace("。", "");
			ques = ques.Replace("？", "");

			// Polish hell pronouns
			ques = ques.Replace("啥", "什么");
			ques = ques.Replace("神马", "什么");

			// Polish OR logic
			if (ques.Contains("或") ||
				ques.Contains("还是") ||
				ques.Contains("要么"))
			{
				fOR = true;

				ques = ques.Replace("还是", "或");
				ques = ques.Replace("或者", "或");
				ques = ques.Replace("或是", "或");
				ques = ques.Replace("要么", "或");

				// Cut the substring between two '或'
				var pos1 = ques.IndexOf('或');
				var pos2 = ques.IndexOf('或', pos1 + 1);
				while (pos2 != -1)
				{
					opts.Add(ques.Substring(pos1 + 1, pos2 - pos1 - 1));
					if (pos1 >= 1)
						ques = ques.Substring(0, pos1 - 1) + ques.Substring(pos2);
					else
						ques = ques.Substring(1);

					pos1 = ques.IndexOf('或');
					pos2 = ques.IndexOf('或', pos1 + 1);
				}
			}
		}

		// Get Wordsegs from ltp-cloud
		// Remark: throw exception if not succeed...
		//public static Word[] AnalyzeQues(string ques)
		//{
		//	HttpClient client = new HttpClinet();
		//	Word[][][] readresult = null;

		//	var content = new FormUrlEncodedContent(new[]
		//		{
		//			new KeyValuePair<string, string>("api_key", "A5X7b0L3TO2Luj1drNOA0hWJUPTMIjYuCgYkEBBq"),
		//			new KeyValuePair<string, string>("pattern", "all"),
		//			new KeyValuePair<string, string>("format", "json"),
		//			new KeyValuePair<string, string>("text", ques)
		//		});

		//	client.PostAsync("http://api.ltp-cloud.com/analysis/", content)
		//		.ContinueWith((task) =>
		//		{
		//			var response = task.Result;
		//			response.EnsureSuccessStatusCode();

		//			var readtask = response.Content.ReadAsAsync<Word[][][]>();
		//			readtask.Wait();
		//			readresult = readtask.Result;
		//		}).Wait();

		//	return readresult[0][0];
		//}

		public static async Task<Word[]> AnalyzeQues(string ques)
		{
			var request = (HttpWebRequest)WebRequest.Create(
				"http://api.ltp-cloud.com/analysis/");
			request.ContentType = "application/x-www-form-urlencoded";
			request.Method = "POST";

			var postData =
				"api_key=A5X7b0L3TO2Luj1drNOA0hWJUPTMIjYuCgYkEBBq" +
				"&pattern=all" +
				"&format=json" +
				"&text=" + ques;
			byte[] byteArray = Encoding.UTF8.GetBytes(postData);

			var requestStream = await request.GetRequestStreamAsync();
			requestStream.Write(byteArray, 0, byteArray.Length);

			var response = await request.GetResponseAsync();

			var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
			var result = JsonConvert.DeserializeObject<Word[][][]>(responseString)[0][0];

			return result;
		}

		// Calc. the similarity score of two strings
		public static double SimiScore(string s, string t)
		{
			int[,] distance = new int[s.Length + 1, t.Length + 1];
			int cost = 0;
			double dist = 0.0;

			if (s.Length == 0) dist = t.Length;
			else if (t.Length == 0) dist = s.Length;
			else
			{
				// Init
				for (int i = 0; i <= s.Length; distance[i, 0] = i++) ;
				for (int j = 0; j <= t.Length; distance[0, j] = j++) ;

				// Find min distance
				for (int i = 1; i <= s.Length; i++)
					for (int j = 1; j <= t.Length; j++)
					{
						cost = (s[i - 1] == t[j - 1] ? 0 : 1);
						distance[i, j] = Math.Min(Math.Min(
							distance[i - 1, j] + 1,
							distance[i, j - 1] + 1),
							distance[i - 1, j - 1] + cost);
						if (i > 1 && j > 1 && s[i - 1] == t[j - 2] && s[i - 2] == t[j - 1])
							distance[i, j] = Math.Min(distance[i, j], distance[i - 2, j - 2]) + cost;
					}
				dist = distance[s.Length, t.Length];
			}

			int maxLen = Math.Max(s.Length, t.Length);
			if (maxLen == 0)
				return 1.0;
			else
			{
				if ((1.0 - dist / maxLen <= 0.9) &&
					(Math.Min(s.Length, t.Length) != 0) &&
					(s.Contains(t) || t.Contains(s)))
					return 0.9;
				return 1.0 - dist / maxLen;
			}
		}

		// Get semantic feature of the question
		// Return {verb, objs, times, locations};
		// each elem of entry is seged by seperator of ' '
		// each keyword of elem is not seged
		// Example: {.., "今天晚上 明天", ..}
		public static string[] GetFeature(Word[] words)
		{
			int head = -1, root = -1;
			List<int> verbs = new List<int>();
			List<int> objs = new List<int>();
			List<int> times = new List<int>();
			List<int> locs = new List<int>();

			// #1.0 Get Verb
			foreach (var i in words)
			{
				// Use HED first
				if (i.pa == -1 && i.re == "HED" && i.pos == "v")
				{
					head = i.id;
					verbs.Add(head);
				}
				// Use else if to prevent duplicate verbs
				else if (i.sempa == -1 && i.semre == "Root" && i.pos == "v")
				{
					// Reduce the Root / HED hell
					root = i.id;
					verbs.Add(i.id);
				}
			}

			if (head == -1)
			{
				if (root == -1)
					// Fatal Error: No Verb
					goto tag_Error_Syntax;
				else
					head = root;
			}

			// #1.1 Iteratively find Possible verbs
			var fAgain = true;
			var verb = head;

			while (fAgain)
			{
				fAgain = false;
				foreach (var i in words)
					if (i.sempa == verb &&
						(i.semre == "eSucc" || i.semre == "ePurp" || i.semre == "eProg"))
					{
						verbs.Add(i.id);
						verb = i.id;
						fAgain = true;
						break;
					}
			}

			// #1.2 Find Core Verb
			foreach (var i in verbs)
			{
				// Judge from words around
				if (i - 1 >= 0 && IsHow(words[i - 1].cont))
				{
					verb = i;
					// Find first one to avoid: 怎么考虑选什么
					break;
				}
				else if (i + 1 < words.Count() && IsWhat(words[i + 1].cont))
				{
					if (IsGo(words[i].cont) &&
						(i + 2 < words.Count() && words[i + 2].pos == "v"))
					{
						if (i + 3 < words.Count() &&
							(words[i + 3].sempa == i + 2 || words[i + 3].sempa == i) &&
								(words[i + 3].semre == "eSucc" ||
								words[i + 3].semre == "ePurp" ||
								words[i + 3].semre == "eProg"))
						{
							// Example: 国庆到哪里去玩
							verb = i + 3;
							break;
						}
						else
						{
							// Example: 国庆去哪儿玩
							verb = i + 2;
							break;
						}
					}
					else
					{
						verb = i;
						break;
					}
				}
			}

			// #2 Extract Objs / Time / Location
			foreach (var i in words)
			{
				// Find Stuff around Verb
				if (i.sempa == verb)
					switch (i.semre)
					{
					case "Pat":
					case "Cont":
					case "Dir":
					case "Prod":
					case "Bleg":
					case "Tool":
						objs.Add(i.id);
						break;

					case "Time":
						times.Add(i.id);
						break;

					case "Loc":
						locs.Add(i.id);
						break;
					}
			}

			string strObjs = "";
			string strTimes = "";
			string strLocs = "";

			// #3.1 Enrich objs' details
			foreach (var t in objs)
			{
				// No need to store pronouns
				string str = !IsWhat(words[t].cont) ? words[t].cont : "";

				for (int i = t; i >= 0; --i)
					if (words[i].sempa == t &&
						(words[i].semre == "Feat" ||
						 words[i].semre == "dFeat" ||
						 words[i].semre == "Host" ||
						 words[i].semre == "Nmod") &&
						!IsWhat(words[i].cont))
						str = words[i].cont + str;

				strObjs += (str + ' ');
			}
			if (strObjs.Length > 0)
				strObjs = strObjs.Remove(strObjs.Length - 1);

			// #3.2 Enrich times' details
			foreach (var t in times)
			{
				// No need to store pronouns
				string str = !IsWhen(words[t].cont) ? words[t].cont : "";

				for (int i = t; i >= 0; --i)
					if (words[i].sempa == t && words[i].semre == "Tmod" && !IsWhen(words[i].cont))
						str = words[i].cont + str;

				strTimes += (str + ' ');
			}
			if (strTimes.Length > 0)
				strTimes = strTimes.Remove(strTimes.Length - 1);

			// #3.3 Enrich locs' details
			foreach (var t in locs)
			{
				// No need to store pronouns
				string str = !IsWhere(words[t].cont) ? words[t].cont : "";

				for (int i = t; i >= 0; --i)
					if (words[i].sempa == t && words[i].semre == "Nmod" && !IsWhere(words[i].cont))
						str = words[i].cont + str;

				strLocs += (str + ' ');
			}
			if (strLocs.Length > 0)
				strLocs = strLocs.Remove(strLocs.Length - 1);

			// #4 Compose result
			return new string[]
			{
				words[verb].cont,
				strObjs,
				strTimes,
				strLocs
			};

			// #0 Syntax Error
			tag_Error_Syntax:
			return new string[] { "null", "null", "null", "null" };
		}

		public static Ques BestMatch(ObservableCollection<Ques> queses, string[] feature, string ques)
		{
			Ques bestQues = null;
			double score = 0.0, maxScore = 0.0;
			double[] facotrs = new double[] { 0.25, 0.45, 0.1, 0.1, 0.1 };

			foreach (var q in queses)
			{
				score = 0.0;
				if (q.feature != null && q.feature[0] != "null")
				{
					var i = 0;
					for (i = 0; i < 4; i++)
						score += SimiScore(feature[i], q.feature[i]) * facotrs[i];

					score += SimiScore(ques, q.question) * facotrs[i];
				}
				else
				{
					// The question has not got feature
					score = SimiScore(ques, q.question);
				}

				if (score > maxScore)
				{
					maxScore = score;
					bestQues = q;
				}
			}

			return maxScore >= 0.85 ? bestQues : null;
		}
	}
}
