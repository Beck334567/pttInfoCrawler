﻿using HtmlAgilityPack;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using pttInfoCrawler.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pttInfoCrawler
{
    public class NotifyTaskManager : IHostedService, IDisposable
    {
        private static Timer _timer;

        
        //private static List<SearchItem> SearchItemList;
        private int execCount = 0;

        public List<PttInfo> resultList = new List<PttInfo>();
        public static List<PttInfo> dayPttInfoList = new List<PttInfo>();

        public NotifyTaskManager()
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
                TimeSpan.FromSeconds(30));
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
                //var textMessage = CreateJsonMessage(hsinchuResult);
                //testdb();
                await SendMessageAsync(hsinchuResult);
            }
            Interlocked.Decrement(ref execCount);
        }
        /*
        private void testdb()
        {
            String connectionString = "Server=ec2-44-197-94-126.compute-1.amazonaws.com;Database=d50j39gi33lgk3;User Id=reunrpslvjomhy;Password=bf6f8a58bdabef6cbf066595d34d478c65ba3c7b40a178739a1bf79e21cbcca0;SslMode=Require;Trust Server Certificate=true;";

            NpgsqlConnection conn = new NpgsqlConnection(connectionString);

            NpgsqlCommand cmd = new NpgsqlCommand("set client_encoding TO big5", conn);

            conn.Open(); 

            cmd.ExecuteNonQuery();
            var sqlCommand = "select * from pttinfo;";
            NpgsqlCommand command = new NpgsqlCommand(sqlCommand, conn);

            NpgsqlDataAdapter adapter = new NpgsqlDataAdapter();

            adapter.SelectCommand = command;
            DataTable table = new DataTable();

            table.Locale = System.Globalization.CultureInfo.InvariantCulture;

            adapter.Fill(table);
            var url = table.Rows[0]["url"];
            
        }
        */
        private string CreateJsonMessage(string message)
        {
            //Myself : Uc50a95a9ba953789b4fdaeb713227780
            //Sister : U13d5ce9b8bc7a642b96caa572a12b701
            var messageObject = new
            {

                message = "test0323"
               
            };
          
            string jsonMessage = JsonConvert.SerializeObject(messageObject);
            return jsonMessage;
        }

        private async Task SendMessageAsync(string message)
        {
            //HttpClient client = new HttpClient();
            //var channelAccessToken = "dV8lQu4BTtChIBGe4r0LBZZ6HTwaZ0lvSdV47raywPF";
            
            //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + channelAccessToken);
            //HttpContent contentPost = new StringContent(message, Encoding.UTF8, "application/x-www-form-urlencoded");
            //HttpResponseMessage responseMessage = await client.PostAsync("https://notify-api.line.me/api/notify", contentPost);
            //
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://notify-api.line.me/api/notify"))

                {
                    //DDAGHquh90KklCeWWAOcYjP4UHKB0fH8dVFFLYQ6vEL 姊
                    //3tPPuAWfTMpWo4RJyk9RkHNi0XEbXenmDCA1EaNezXx 自己測試
                    request.Headers.TryAddWithoutValidation("Authorization", $"Bearer DDAGHquh90KklCeWWAOcYjP4UHKB0fH8dVFFLYQ6vEL");

                    request.Content = new StringContent($"message={message}");

                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                    var response = await httpClient.SendAsync(request);
                }
            }

        }

        private string GetHsinchuByTask()
        {
            HtmlWeb webClient = new HtmlWeb();
            var HsinchuUrl = "https://www.ptt.cc/bbs/Hsinchu/index.html";
            HtmlDocument doc = webClient.Load(HsinchuUrl);
            var pttInfoList = new List<PttInfo>();
            var dayPttInfoTitleList = dayPttInfoList.Select(x => x.title).ToList();
            var dayPttInfoUrlList = dayPttInfoList.Select(x => x.url).ToList();
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
                    if ((pttInfo.title.Contains("贈送")|| pttInfo.title.Contains("東門水餃")) && 
                        !dayPttInfoUrlList.Contains(pttInfo.url)&&
                        !pttInfo.title.Contains("洽") &&
                        !pttInfo.title.Contains("恰") &&
                        !pttInfo.title.Contains("暫") &&
                        !pttInfo.title.Contains("送出") &&
                        !pttInfo.title.Contains("已") && 
                        !pttInfo.title.Contains("Re"))
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
                resultStr += "\n貼文日期 : " + info.date.ToString("MM/dd") + "\n" + info.title + "\n 推文數:" + info.tweetCount + "\n" + info.url + "\n";
            }
            //resultStr = "查詢//新竹版//贈送" + "\r\n" + resultStr;

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
