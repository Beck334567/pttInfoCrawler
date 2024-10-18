using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using pttInfoCrawler.Model;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace pttInfoCrawler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LineController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post(dynamic request)
        {
            string x = Convert.ToString(request);
            var channelAccessToken = "c4RXeRfxeAqTmt81bq3AZH/0jSooqiUjxsHAoMGR6oHNV4alsM3UOP+h3gi8zH3SpW8EG5kpb4wmBH9EQGgn3+v4aBvNZxsEZ6xvd5bOCZgMv83vFNpGeln/Gwc/rYkygjShITKe3L275jjRkE2iSgdB04t89/1O/w1cDnyilFU=";
            // 收到訊息解成物件
            var receivedMessage = JsonConvert.DeserializeObject<WebhookEvent>(x);
            // 取得文字訊息
            string message = receivedMessage.events[0].message.text;
            // 取得回應TOKEN
            var replyToken = receivedMessage.events[0].replyToken;
            // 取得Ptt爬文結果
            var pttresult = GetPTTContent(message);
            // 建立回傳物件
            var replyMessage = new
            {
                replyToken = replyToken,
                messages = new object[]
                {
                    new
                    {
                        type = "text",
                        text = pttresult
                    }
                }
            };

            //建立 HttpClient
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + channelAccessToken);
            string json = JsonConvert.SerializeObject(replyMessage);
            HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");
            // 使用reply api 回傳
            HttpResponseMessage responseMessage = await client.PostAsync("https://api.line.me/v2/bot/message/reply", contentPost);
            HttpContent content = responseMessage.Content;
            return Ok();
        }

        private string GetPTTContent(string queryString)
        {
            HtmlWeb webClient = new HtmlWeb();
            var requestUrl = "";
            var HsinchuUrl = "https://www.ptt.cc/bbs/Hsinchu/index.html";
            var LifeismoneyUrl = "https://www.ptt.cc/bbs/Lifeismoney/index.html";
            if (queryString == "新竹")
            {
                requestUrl = HsinchuUrl;
            }
            else if (queryString == "生活")
            {
                requestUrl = LifeismoneyUrl;
            }
            else
            {
                return "尚未加入此版";
            }

            HtmlDocument doc = webClient.Load(requestUrl);
            var result = "";
            if (queryString == "新竹")
            {
                for (int i = 2; i <= 40; i++)
                {
                    // 標題
                    HtmlNodeCollection title = doc.DocumentNode.SelectNodes($"//*[@id='main-container']/div[2]/div[{i}]/div[2]/a");
                    // 連結
                    HtmlNodeCollection link = doc.DocumentNode.SelectNodes($"//*[@id='main-container']/div[2]/div[{i}]/div[2]/a");
                    // 日期
                    HtmlNodeCollection date = doc.DocumentNode.SelectNodes($"//*[@id='main-container']/div[2]/div[{i}]/div[3]/div[3]");
                    if (title != null && link != null && date != null)
                    {
                        if (title[0].InnerText.ToString().Contains("贈送"))
                        {
                            var url = link[0].OuterHtml.ToString();
                            var first = url.IndexOf('"') + 1;
                            var last = url.LastIndexOf('"') - 9;
                            url = "https://www.ptt.cc/" + url.Substring(first, last);
                            result += title[0].InnerText.ToString() + "\r" + date[0].InnerText.ToString() + "\n" + url + "\n";
                        }
                    }
                }
                result = "查詢//新竹版//贈送" + "\n" + result;
            }
            else
            {
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
            }

            return result;
        }
    }
}