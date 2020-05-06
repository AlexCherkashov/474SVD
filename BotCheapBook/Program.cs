using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VkNet;
using System.Net;
using VkNet.Model;
using VkNet.Enums.Filters;
using VkNet.Utils;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Diagnostics;
using System.Data;
using System.Data.SqlClient;
using VkNet.Model.Keyboard;
using VkNet.Enums.SafetyEnums;
using Newtonsoft.Json;
using System.IO;

namespace BotCheapBook
{

    class Program
    {
        private static Random random = new Random();
        private static SqlConnection sql;

        static void Main()
        {
            string connectionString = File.ReadAllText("connectionString.txt");
            sql = new SqlConnection(connectionString);
            sql.Open();

            VkApi vk = new VkApi();
            var webClient = new WebClient() { Encoding = Encoding.UTF8 };

            vk.Authorize(new ApiAuthParams
            {
                ApplicationId = 7443996,
                Login = File.ReadAllText("login.txt"),
                Password = File.ReadAllText("password.txt"),
                Settings = Settings.All | Settings.All
            });
            Console.WriteLine("Авторизация прошла");
            var Commands = new string[8] { "Лабиринт", "book24", "my-shop", "Дом Книги",
                                            "Республика", "Добавить", "Удалить", "Начать" };

            IKeyboard keyboardSites = new KeyboardSites();
            IKeyboard keyboardMenu = new KeyboardMenu();
            IKeyboard keyboardBestSellers = new KeyboardBestSellers();
            IKeyboard keyboardMyList = new KeyboardMyList();

            string sites = keyboardSites.GetJsonKeyboard();
            string menu = keyboardMenu.GetJsonKeyboard();
            string bestSellers = keyboardBestSellers.GetJsonKeyboard();
            string myList = keyboardMyList.GetJsonKeyboard();

            var param = new VkParameters() { };
            param.Add("group_id", "194889296");

            dynamic responseLongPoll = JObject.Parse(vk.Call("groups.getLongPollServer", param).RawJson);

            string json = String.Empty;
            string url = String.Empty;

            while (true)
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
                JToken item = null;
                try
                {
                    item = JObject.Parse(json)["updates"].ToList().Last();
                }
                catch { };


                if (item != null && item["type"].ToString() == "message_new")
                {
                    string token = "55761d91e497ca618d2e2166ba45b644a8cda06b0984c97efe9db936aba16676664fa47519fc27ea2dfbd";
                    string urlBotMsg = $"https://api.vk.com/method/messages.send?v=5.103&access_token=" + token + "&user_id=";
                    var msg = item["object"]["message"]["text"].ToString();
                    string id = item["object"]["message"]["peer_id"].ToString();
                    Console.WriteLine($"{msg}");

                    if (Commands.Contains(msg))
                        continue;

                    switch (msg)
                    {
                        case "Сайты":
                            SendKeyboard(webClient, urlBotMsg, id, "Обновление клавиатуры", sites);
                            Sites.Run(url, json, webClient, responseLongPoll, menu, urlBotMsg, sql);
                            continue;
                        case "Бестселлеры":
                            SendKeyboard(webClient, urlBotMsg, id, "Обновление клавиатуры", bestSellers);
                            Bestsellers.Run(url, json, webClient, responseLongPoll, menu, urlBotMsg, sql);
                            continue;
                        case "Что хочу прочесть":
                            SendKeyboard(webClient, urlBotMsg, id, MyList.GetMyList(id, sql), myList);
                            MyList.Books(url, webClient, responseLongPoll, menu, urlBotMsg, sql);
                            continue;
                        case "Назад":
                            SendKeyboard(webClient, urlBotMsg, id, "Обновление клавиатуры", menu);
                            continue;
                    }

                    BookSearch.GetBook(webClient, urlBotMsg, id, msg, sql);
                    Thread.Sleep(200);
                }
            }
        }

        public static void SendMessage(WebClient webClient, string urlBotMsg, string id, string message)
        {
            webClient.DownloadString(
                string.Format(urlBotMsg + "{0}&message={1}&random_id={2}",
                id, message,
                random.Next()));
        }

        public static void SendKeyboard(WebClient webClient, string urlBotMsg, string id, string message, string keyboard)
        {
            webClient.DownloadString(
                string.Format(urlBotMsg + "{0}&message={1}&random_id={2}&keyboard={3}",
                id, message,
                random.Next(), keyboard));
        }

        public static Tuple<int, string> GetLink(string bookName)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            BookShop labirint = new Labirint();
            BookShop book24 = new Book24();
            BookShop myShop = new MyShop();
            BookShop domKnigi = new DomKnigi();
            BookShop respublica = new Respublica();

            var array = new List<Tuple<int, string>>
            {
                labirint.GetBestBook(bookName),
                book24.GetBestBook(bookName),
                myShop.GetBestBook(bookName),
                domKnigi.GetBestBook(bookName),
                respublica.GetBestBook(bookName)
            };

            stopWatch.Stop();
            var time = stopWatch.Elapsed;
            Console.WriteLine(time);

            var books = new List<Tuple<int, string>>();
            foreach (var book in array)
            {
                if (book != null)
                    books.Add(book);
            }
            return books.OrderBy(x => x.Item1).First();
        }
    }
}
