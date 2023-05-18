using HtmlAgilityPack;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ZXing;

namespace Helper_1080.Helper
{

    public class ForumApi
    {
        public HttpClient Client;

        public ForumApi(bool useCookie = true)
        {
            string Cookie = AppSettings.Cookie;

            var handler = new HttpClientHandler { UseCookies = false };
            Client = new HttpClient(handler);
            Client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36 Edg/105.0.1343.53");
            Client.DefaultRequestHeaders.Add("referer", UrlCombine(AppSettings.BaseUrl, "forum.php"));

            if (useCookie && !string.IsNullOrEmpty(Cookie))
            {
                Client.DefaultRequestHeaders.Add("Cookie", Cookie);
            }
        }

        public static string UrlCombine(string uri1, string uri2)
        {
            Uri baseUri = new Uri(uri1);
            Uri myUri = new Uri(baseUri, uri2);
            return myUri.ToString();
        }

        public async Task<List<string>> getDownLogToday()
        {
            List<string> downList = new();

            DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0);
            startTime = startTime.AddSeconds(DateTimeOffset.Now.ToUnixTimeSeconds()).ToLocalTime();
            string todayDate = string.Format("{0:yyyy-MM-dd}", startTime);

            for (int i = 0; i < 3; i++)
            {
                var newDict = await requestDownLogAsync(i+1);

                if (newDict?.Count > 1)
                {
                    if (newDict.ContainsKey(todayDate))
                    {
                        downList.AddRange(newDict[todayDate]);
                    }
                    else
                    {
                        continue;
                    }

                    break;
                }
                else if(newDict?.Count == 1)
                {
                    continue;
                }
                else if (newDict?.Count == 0)
                {
                    break;
                }
            }

            return downList;
        }

        private async Task<Dictionary<string, List<string>>> requestDownLogAsync(int page=1)
        {
            Dictionary<string, List<string>> dateDict = new();

            string pageStr = page == 1 ?"" : $"&page={page}";

            var checkDownLogUrl = UrlCombine(AppSettings.BaseUrl, $"plugin.php?id=xiaomy_attachlog:list{pageStr}");

            HttpResponseMessage response;
            try
            {
                response = Client.GetAsync(checkDownLogUrl).Result;
            }
            catch (AggregateException e)
            {
                Console.WriteLine("网络异常", "检查下载记录时出现异常：", e.Message);

                return dateDict;
            }

            var strResult = await response.Content.ReadAsStringAsync();
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(strResult);

            var threadlisttableNode = htmlDoc.DocumentNode.SelectSingleNode("//table[@id='threadlisttableid']");
            //搜索无果，退出
            if (threadlisttableNode == null) return null;

            //搜索查找当前页的下载记录
            var AttributeNodes = threadlisttableNode.SelectNodes("tbody[contains(@id,'normalthread')]");

            foreach (var node in AttributeNodes)
            {
                var nodes = node.SelectNodes("tr/td");
                string downDate = nodes[nodes.Count - 1].InnerText.Trim().Substring(0, 10);


                string title = node.SelectSingleNode("tr/th[@class='common']/a")?.InnerText.Trim();

                if (dateDict.ContainsKey(downDate))
                {
                    dateDict[downDate].Add(title);
                }
                else
                {
                    dateDict[downDate] = new List<string>() { title };

                }
            }

            return dateDict;

        }
    }
}
