using HtmlAgilityPack;
using System;
using System.Collections.Generic;
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
        public string Cookie;

        public ForumApi(string cookie = "", bool useCookie = true)
        {
            Cookie = cookie;

            var handler = new HttpClientHandler { UseCookies = false };
            Client = new HttpClient(handler);
            Client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36 Edg/105.0.1343.53");
            Client.DefaultRequestHeaders.Add("referer", UrlCombine(AppSettings.BaseUrl, "forum.php"));

            if (useCookie && !string.IsNullOrEmpty(cookie))
            {
                //cookie不为空且可用
                if (!string.IsNullOrEmpty(Cookie))
                {
                    Client.DefaultRequestHeaders.Add("Cookie", Cookie);
                }
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
            var downLogList = new List<string>();

            var checkDownLogUrl = UrlCombine(AppSettings.BaseUrl, "plugin.php?id=xiaomy_attachlog:list");

            HttpResponseMessage response;
            try
            {
                response = Client.GetAsync(checkDownLogUrl).Result;
            }
            catch (AggregateException e)
            {
                Console.WriteLine("网络异常", "检查下载记录时出现异常：", e.Message);

                return downLogList;
            }

            var strResult = await response.Content.ReadAsStringAsync();
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(strResult);

            var threadlisttableNode = htmlDoc.DocumentNode.SelectSingleNode("//table[@id='threadlisttableid']");
            //搜索无果，退出
            if (threadlisttableNode == null) return null;

            return downLogList;
        }
    }
}
