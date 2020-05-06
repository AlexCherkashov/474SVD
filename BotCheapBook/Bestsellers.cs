using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotCheapBook
{
    public class Bestsellers
    {
        public static string GetBestSellers(string url, string checkBooks, char check)
        {
            int length = checkBooks.Length;

            try
            {
                HttpClient httpClient = new HttpClient();
                string data = httpClient.GetStringAsync(url).Result;
                string findPrice = data.Substring(0, length);
                int count = 0;
                List<string> books = new List<string>();
                for (var i = 0; i < data.Length - length; i++)
                {
                    if (checkBooks == findPrice.ToString())
                    {
                        books.Add(GetData(i + length + 1, data, check));
                        count++;
                    }
                    findPrice = findPrice.Substring(1) + data[i + length];
                    if (count == 10)
                        break;
                }

                string result = "";
                count = 1;
                foreach (var book in books)
                {
                    result += count + ". " + book + "\n";
                    count++;
                }
                return result;
            }
            catch { return null; }
        }

        private static string GetData(int i, string data, char check)
        {
            string book = "";
            while (data[i] != check)
            {
                book += data[i];
                i++;
            }
            return book;
        }

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
                                Program.SendMessage(webClient, urlBotMsg, id,
                                    Bestsellers.GetBestSellers("https://www.labirint.ru/rating/", "data-name=", '"'));
                                Program.SendMessage(webClient, urlBotMsg, id, "Источник - https://www.labirint.ru/rating/");
                                continue;
                            case "book24":
                                Program.SendMessage(webClient, urlBotMsg, id,
                                    Bestsellers.GetBestSellers("https://book24.ru/knigi-bestsellery/", "data-product-name=", '"'));
                                Program.SendMessage(webClient, urlBotMsg, id, "Источник - https://book24.ru/knigi-bestsellery/");
                                continue;
                            case "Дом Книги":
                                Program.SendMessage(webClient, urlBotMsg, id,
                                    Bestsellers.GetBestSellers("https://www.spbdk.ru/top/khity-prodazh/", "snippet__title\"", '<'));
                                Program.SendMessage(webClient, urlBotMsg, id, "Источник - https://www.spbdk.ru/top/khity-prodazh/");
                                continue;
                            case "Республика":
                                Program.SendMessage(webClient, urlBotMsg, id,
                                    Bestsellers.GetBestSellers("https://www.respublica.ru/knigi/bestsellery", "><a alt=", '"'));
                                Program.SendMessage(webClient, urlBotMsg, id, "Источник - https://www.respublica.ru/knigi/bestsellery/");
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
