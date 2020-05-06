using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotCheapBook
{
    public class Sites
    {
        public static void Run(string url, string json, WebClient webClient, dynamic responseLongPoll, string menu, string urlBotMsg, SqlConnection sql)
        {
            bool flag = true;
            while (flag)
            {
                string server = responseLongPoll.response.server.ToString();
                string key = responseLongPoll.response.key.ToString();
                string ts = responseLongPoll.response.ts.ToString();
                url = string.Format("{0}?act=a_check&key={1}&ts={2}&wait=25",
                        server,
                        key,
                        json != String.Empty ? JObject.Parse(json)["ts"].ToString() : ts);
                json = webClient.DownloadString(url);

                var jsonMsg = json.IndexOf(":[]}") > -1 ? "" : $"{json} \n";
                var col = JObject.Parse(json)["updates"].ToList();

                foreach (var item in col)
                {
                    if (item["type"].ToString() == "message_new")
                    {
                        var msg = item["object"]["message"]["text"].ToString();
                        string id = item["object"]["message"]["peer_id"].ToString();
                        Console.WriteLine($"{msg}");

                        switch (msg)
                        {
                            case "Лабиринт":
                                Program.SendMessage(webClient, urlBotMsg, id, "https://www.labirint.ru/");
                                continue;
                            case "book24":
                                Program.SendMessage(webClient, urlBotMsg, id, "https://book24.ru/");
                                continue;
                            case "my-shop":
                                Program.SendMessage(webClient, urlBotMsg, id, "https://my-shop.ru/");
                                continue;
                            case "Дом Книги":
                                Program.SendMessage(webClient, urlBotMsg, id, "https://www.spbdk.ru/");
                                continue;
                            case "Республика":
                                Program.SendMessage(webClient, urlBotMsg, id, "https://www.respublica.ru/");
                                continue;
                            case "Назад":
                                Program.SendKeyboard(webClient, urlBotMsg, id, "Возврат", menu);
                                flag = false;
                                continue;
                        }

                        BookSearch.GetBook(webClient, urlBotMsg, id, msg, sql);
                        Thread.Sleep(200);
                    }
                }
            }
        }
    }
}
