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
using VkNet.Model.Keyboard;
using VkNet.Enums.SafetyEnums;
using Newtonsoft.Json;

namespace BotCheapBook
{
    class Program
    {
        private static Random random = new Random();

        static void Main()
        {

            VkApi vk = new VkApi();
            var webClient = new WebClient() { Encoding = Encoding.UTF8 };

            vk.Authorize(new ApiAuthParams
            {
                ApplicationId = 7443996,
                Login = "XXX",
                Password = "XXX",
                Settings = Settings.All | Settings.All
            });
            Console.WriteLine("Авторизация прошла");

            var BooksStores = new string[6] { "Буквоед", "book24", "my-shop", "Дом Книги", "Республика", "Лабиринт" };

            var buttons = new List<MessageKeyboardButton>();

            for (var i = 0; i < 6; i++)
            {
                buttons.Add(new MessageKeyboardButton
                {
                    Action = new MessageKeyboardButtonAction
                    {
                        Type = KeyboardButtonActionType.Text,
                        Label = BooksStores[i]
                    },
                    Color = KeyboardButtonColor.Positive
                });
            }


            var keyboard = new MessageKeyboard
            {
                OneTime = false,
                Buttons = new List<List<MessageKeyboardButton>>
            {
                new List<MessageKeyboardButton>
                {
                    buttons[0], buttons[1]
                },
                new List<MessageKeyboardButton>
                {
                    buttons[2], buttons[3]
                },
                new List<MessageKeyboardButton>
                {
                    buttons[4], buttons[5]
                }
            }
            };

            string jsonKeyBoard = JsonConvert.SerializeObject(keyboard);

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
                var col = JObject.Parse(json)["updates"].ToList();

                foreach (var item in col)
                {
                    if (item["type"].ToString() == "message_new")
                    {
                        string token = "55761d91e497ca618d2e2166ba45b644a8cda06b0984c97efe9db936aba16676664fa47519fc27ea2dfbd";
                        string urlBotMsg = $"https://api.vk.com/method/messages.send?v=5.103&access_token=" + token + "&user_id=";
                        var msg = item["object"]["message"]["text"].ToString();
                        string id = item["object"]["message"]["peer_id"].ToString();
                        Console.WriteLine($"{msg}");

                        switch (msg)
                        {
                            case "Буквоед":
                                SendMessage(webClient, urlBotMsg, id, "https://www.bookvoed.ru/");
                                continue;
                            case "book24":
                                SendMessage(webClient, urlBotMsg, id, "https://book24.ru/");
                                continue;
                            case "my-shop":
                                SendMessage(webClient, urlBotMsg, id, "https://my-shop.ru/");
                                continue;
                            case "Дом Книги":
                                SendMessage(webClient, urlBotMsg, id, "https://www.spbdk.ru/");
                                continue;
                            case "Республика":
                                SendMessage(webClient, urlBotMsg, id, "https://www.respublica.ru/");
                                continue;
                            case "Лабиринт":
                                SendMessage(webClient, urlBotMsg, id, "https://www.labirint.ru/");
                                continue;
                        }

                        webClient.DownloadString(
                            string.Format(urlBotMsg + "{0}&message={1}&random_id={2}&keyboard={3}",
                            id, "Поиск...",
                            random.Next(), jsonKeyBoard));

                        var data = GetLink(msg);

                        if (data == null)
                        {
                            SendMessage(webClient, urlBotMsg, id, "Прости, я ничего не нашёл");
                        }
                        else
                        {
                            SendMessage(webClient, urlBotMsg, id, "Эту книгу можно купить за " + data.Item1 + " рублей");
                            SendMessage(webClient, urlBotMsg, id, data.Item2);
                        }
                        Thread.Sleep(200);
                    }
                }
            }
        }

        private static void SendMessage(WebClient webClient, string urlBotMsg, string id, string message)
        {
            webClient.DownloadString(
                string.Format(urlBotMsg + "{0}&message={1}&random_id={2}",
                id, message,
                random.Next()));
        }

        private static Tuple<int, string> GetLink(string bookName)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var array = new List<Tuple<int, string>>();
            array.Add(Labirint.ByLabirint(bookName));
            array.Add(Book24.ByBook24(bookName));
            array.Add(MyShop.ByMyShop(bookName));
            array.Add(DomKnigi.ByDomKnigi(bookName));
            array.Add(Respublica.ByMyShop(bookName));

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
