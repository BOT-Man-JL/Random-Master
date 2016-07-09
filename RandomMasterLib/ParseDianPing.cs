using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RandomMaster
{
	public class ParseDianPing
	{
		public class DianPingData
		{
			public string shopLink { private get; set; }
			public string title { private get; set; }
			public string imgSrc { private get; set; }
			public string addr { private get; set; }

			public string link
			{
				get { return "http://www.dianping.com/shop/" + shopLink; }
			}

			public string name
			{
				get { return title; }
			}

			public string address
			{
				get { return addr; }
			}

			//public Bitmap img
			//{

			//} 
		}

		public async Task<DianPingData> GetOne(string keyword)
		{
			var list = await Parse(keyword);
			return list[new Random(DateTime.Now.Millisecond).Next(list.Count)];
		}

		public async Task<List<DianPingData>> Parse(string keyword)
		{
			var htmlPage = await RetrivePage(keyword);
			return ParsePage(htmlPage);
		}

		private async Task<string> RetrivePage(string keyword)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.UserAgent
					.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)"
					+ " Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586");
				var result = await client.GetAsync("http://www.dianping.com/search/keyword/2/0_" + keyword);
				try
				{
					result.EnsureSuccessStatusCode();
					var htmlPage = await result.Content.ReadAsStringAsync();
					return htmlPage;
				}
				catch (Exception e)
				{
					throw new Exception(e.Message + " - Get DianPing.com Failed");
				}
			}
		}

		private List<DianPingData> ParsePage(string htmlPage)
		{
			var ret = new List<DianPingData>();

			// Issue: corrupt first item
			var matches = Regex.Matches(htmlPage,
				"<li(.*?)class=\"pic\"(.*?)class=\"tag-addr\"(.*?)<a (.*?)</a>(.*?)</li>", RegexOptions.Singleline);
			foreach (Match match in matches)
			{
				var usefulPart = match.Value;
				var href = Regex.Match(usefulPart, "href=\"/shop/(.*?)\"").Groups[1].Value;
				var alt = Regex.Match(usefulPart, "alt=\"(.*?)\"").Groups[1].Value;
				var src = Regex.Match(usefulPart, "data-src=\"(.*?)\"").Groups[1].Value;
				var addr = Regex.Match(usefulPart, "<span class=\"addr\">(.*?)</span>").Groups[1].Value;

				// Discard corrupt data
				if (href == "" || alt == "" || src == "" || addr == "")
					continue;

				ret.Add(new DianPingData { shopLink = href, title = alt, imgSrc = src, addr = addr });
			}

			return ret;
		}
	}
}
