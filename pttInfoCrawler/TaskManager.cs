using HtmlAgilityPack;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using pttInfoCrawler.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pttInfoCrawler
{
    public class TaskManager : IHostedService, IDisposable
    {
        private static Timer _timer;

        //todo 接到指令列清單
        //private static List<SearchItem> SearchItemList;
        private int execCount = 0;

        public List<PttInfo> resultList = new List<PttInfo>();
        public List<PttInfo> dayPttInfoList = new List<PttInfo>();

        public TaskManager()
        {
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //調整Timer為永不觸發，停用定期排程
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(ExecuteTask, null,
                TimeSpan.Zero,
                //設定時間
                TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        public async void ExecuteTask(object state)
        {
            var hsinchuResult = GetHsinchuByTask();
            if (string.IsNullOrEmpty(hsinchuResult)) return;

            //利用 Interlocked 計數防止重複執行
            Interlocked.Increment(ref execCount);
            if (execCount == 1)
            {
                var textMessage = CreateJsonMessage(hsinchuResult);
                await SendMessageAsync(textMessage);
            }
            Interlocked.Decrement(ref execCount);
        }

        private string CreateJsonMessage(string message)
        {
            //Myself : Uc50a95a9ba953789b4fdaeb713227780
            //Sister : U13d5ce9b8bc7a642b96caa572a12b701
            var messageObject = new
            {
                to = new object[]
                {
                    "Uc50a95a9ba953789b4fdaeb713227780" //,"U13d5ce9b8bc7a642b96caa572a12b701"
                },
                messages = new object[]
                {
                   new
                   {
                       type = "text",
                       text = "查詢時間:"+DateTime.Now.ToString()+"\n"+message
                   }
                }
            };
            string jsonMessage = JsonConvert.SerializeObject(messageObject);
            return jsonMessage;
        }

        private async Task SendMessageAsync(string message)
        {
            HttpClient client = new HttpClient();
            var channelAccessToken = "c4RXeRfxeAqTmt81bq3AZH/0jSooqiUjxsHAoMGR6oHNV4alsM3UOP+h3gi8zH3SpW8EG5kpb4wmBH9EQGgn3+v4aBvNZxsEZ6xvd5bOCZgMv83vFNpGeln/Gwc/rYkygjShITKe3L275jjRkE2iSgdB04t89/1O/w1cDnyilFU=";
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + channelAccessToken);
            HttpContent contentPost = new StringContent(message, Encoding.UTF8, "application/json");
            HttpResponseMessage responseMessage = await client.PostAsync("https://api.line.me/v2/bot/message/multicast", contentPost);
        }

        private string GetHsinchuByTask()
        {
            HtmlWeb webClient = new HtmlWeb();
            var HsinchuUrl = "https://www.ptt.cc/bbs/Hsinchu/index.html";
            HtmlDocument doc = webClient.Load(HsinchuUrl);
            var pttInfoList = new List<PttInfo>();
            var dayPttInfoTitleList = dayPttInfoList.Select(x => x.title).ToList();
            for (int i = 2; i <= 30; i++)
            {
                // 標題
                HtmlNodeCollection title = doc.DocumentNode.SelectNodes($"//*[@id='main-container']/div[2]/div[{i}]/div[2]/a");
                // 連結
                HtmlNodeCollection link = doc.DocumentNode.SelectNodes($"//*[@id='main-container']/div[2]/div[{i}]/div[2]/a");
                // 日期
                HtmlNodeCollection date = doc.DocumentNode.SelectNodes($"//*[@id='main-container']/div[2]/div[{i}]/div[3]/div[3]");
                // 推文數
                HtmlNodeCollection tweet = doc.DocumentNode.SelectNodes($"//*[@id='main-container']/div[2]/div[{i}]/div[1]/span");

                if (title != null && link != null && date != null)
                {
                    var tweetCount = tweet == null ? "0" : tweet[0].InnerText;
                    if (tweetCount == "爆")
                    {
                        tweetCount = "100";
                    }
                    else if (Int32.TryParse(tweetCount, out var number))
                    {
                    }
                    else { tweetCount = "0"; }

                    var url = link[0].OuterHtml.ToString();
                    var first = url.IndexOf('"') + 1;
                    var last = url.LastIndexOf('"') - 9;
                    url = "https://www.ptt.cc/" + url.Substring(first, last);

                    var pttInfo = new PttInfo()
                    {
                        title = title[0].InnerText.ToString(),
                        url = url,
                        date = Convert.ToDateTime(date[0].InnerText.ToString()),
                        tweetCount = Convert.ToInt16(tweetCount)
                    };
                    if (pttInfo.title.Contains("贈送") && !dayPttInfoTitleList.Contains(pttInfo.title))
                    {
                        pttInfoList.Add(pttInfo);
                        dayPttInfoList.Add(pttInfo);
                    }
                }
            }
            var resultStr = "";
            if (pttInfoList.Count == 0)
            {
                return "";
            }
            foreach (var info in pttInfoList)
            {
                resultStr += info.date.ToString("m") + "\r\n" + info.title + "\n 推文數:" + info.tweetCount + "\r\n" + info.url + "\n";
            }
            resultStr = "查詢//新竹版//贈送" + "\r\n" + resultStr;

            return resultStr;
        }



        private string GetLifeismoneyByTask()
        {
            HtmlWeb webClient = new HtmlWeb();
            var LifeismoneyUrl = "https://www.ptt.cc/bbs/Lifeismoney/index.html";
            HtmlDocument doc = webClient.Load(LifeismoneyUrl);
            var result = "";
            for (int i = 2; i <= 40; i++)
            {
                // 標題
                HtmlNodeCollection title = doc.DocumentNode.SelectNodes($"//*[@id='main-container']/div[2]/div[{i}]/div[2]/a");
                //*[@id="main-container"]/div[2]/div[2]/div[2]/a
                // 連結
                HtmlNodeCollection link = doc.DocumentNode.SelectNodes($"//*[@id='main-container']/div[2]/div[{i}]/div[2]/a");
                // 日期
                HtmlNodeCollection date = doc.DocumentNode.SelectNodes($"//*[@id='main-container']/div[2]/div[{i}]/div[3]/div[3]");
                if (title != null && link != null && date != null)
                {
                    if (title[0].InnerText.ToString().Contains("情報"))
                    {
                        var url = link[0].OuterHtml.ToString();
                        var first = url.IndexOf('"') + 1;
                        var last = url.LastIndexOf('"') - 9;
                        url = "https://www.ptt.cc/" + url.Substring(first, last);
                        result += title[0].InnerText.ToString() + "\r" + date[0].InnerText.ToString() + "\n" + url + "\n";
                    }
                }
            }
            result = "查詢//LifeIsMoney//情報" + "\n" + result;

            return result;
        }
    }
}
