using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BotCheapBook
{
    public class MyList
    {
        public static void Books(string url, WebClient webClient, dynamic responseLongPoll, string menu, string urlBotMsg, SqlConnection sql)
        {
            string json = "";
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
                var item = JObject.Parse(json)["updates"].ToList().Last();


                if (item["type"].ToString() == "message_new")
                {
                    var msg = item["object"]["message"]["text"].ToString();
                    string id = item["object"]["message"]["peer_id"].ToString();
                    Console.WriteLine($"{msg}");

                    switch (msg)
                    {
                        case "Добавить":
                            Program.SendMessage(webClient, urlBotMsg, id, "Напиши название книги");
                            AddToList(url, webClient, responseLongPoll, urlBotMsg, sql);
                            continue;
                        case "Удалить":
                            Program.SendMessage(webClient, urlBotMsg, id, "Напиши название книги");
                            DeleteFromList(url, webClient, responseLongPoll, urlBotMsg, sql);
                            continue;
                        case "Назад":
                            Program.SendKeyboard(webClient, urlBotMsg, id, "Возврат", menu);
                            flag = false;
                            continue;
                    }
                }
            }
        }

        public static void DeleteFromList(string url, WebClient webClient, dynamic responseLongPoll, string urlBotMsg, SqlConnection sql)
        {
            string json = "";
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
                var item = JObject.Parse(json)["updates"].ToList().Last();
                if (item["type"].ToString() == "message_new")
                {
                    var msg = item["object"]["message"]["text"].ToString();
                    string id = item["object"]["message"]["peer_id"].ToString();
                    Console.WriteLine($"{msg}");
                    Program.SendMessage(webClient, urlBotMsg, id, DeleteFromList(id, msg, sql));
                    Program.SendMessage(webClient, urlBotMsg, id, GetMyList(id, sql));
                    return;
                }

            }
        }

        private static string DeleteFromList(string id, string book, SqlConnection sql)
        {
            try
            {
                SqlCommand command = new SqlCommand("SELECT * FROM [MyList] WHERE bookName = @bookName", sql);
                command.Parameters.AddWithValue("bookName", book);
                SqlDataReader sqlReader = command.ExecuteReader();
                if (sqlReader.HasRows)
                {
                    sqlReader.Close();
                    command = new SqlCommand("DELETE FROM [MyList] WHERE peer_id = @peer_id AND bookName = @bookName", sql);
                    command.Parameters.AddWithValue("peer_id", id);
                    command.Parameters.AddWithValue("bookName", book);
                    command.ExecuteNonQuery();
                    return "Успешно";
                }
                else
                {
                    sqlReader.Close();
                    return "У тебя не было такой книги в списке";
                }
            }
            catch
            {
                return "Ошибка";
            }
        }


        public static void AddToList(string url, WebClient webClient, dynamic responseLongPoll, string urlBotMsg, SqlConnection sql)
        {
            string json = "";
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
                var item = JObject.Parse(json)["updates"].ToList().Last();
                if (item["type"].ToString() == "message_new")
                {
                    var msg = item["object"]["message"]["text"].ToString();
                    string id = item["object"]["message"]["peer_id"].ToString();
                    Console.WriteLine($"{msg}");
                    Program.SendMessage(webClient, urlBotMsg, id, AddToList(id, msg, sql));
                    Program.SendMessage(webClient, urlBotMsg, id, GetMyList(id, sql));
                    return;
                }
            }
        }



        private static string AddToList(string id, string book, SqlConnection sql)
        {
            try
            {
                SqlCommand command = new SqlCommand("SELECT * FROM [MyList] WHERE bookName = @bookName", sql);
                command.Parameters.AddWithValue("bookName", book);
                SqlDataReader sqlReader = command.ExecuteReader();
                if (!sqlReader.HasRows)
                {
                    sqlReader.Close();
                    command = new SqlCommand("INSERT INTO [MyList] (peer_id, bookName)" +
                                                "VALUES(@peer_id, @bookName)", sql);
                    command.Parameters.AddWithValue("peer_id", id.Replace(" ", ""));
                    command.Parameters.AddWithValue("bookName", book);
                    command.ExecuteNonQuery();
                    return "Успешно";
                }
                else
                {
                    sqlReader.Close();
                    return "У вас уже есть такая книга";
                }
            }
            catch
            {
                return "Ошибка";
            }
        }

        public static string GetMyList(string id, SqlConnection sql)
        {
            SqlCommand command = new SqlCommand("SELECT * FROM [MyList] WHERE peer_id = @peer_id", sql);
            command.Parameters.AddWithValue("peer_id", id);
            SqlDataReader sqlReader = command.ExecuteReader();
            string myList = "";
            int count = 1;
            if (!sqlReader.HasRows)
            {
                sqlReader.Close();
                return "У нас здесь ничего нет.";
            }
            else
            {
                while (sqlReader.Read())
                {
                    string book = sqlReader[2].ToString();
                    myList += count + ". " + book + "\n";
                    count++;
                }
                sqlReader.Close();
                return myList;
            }
        }
    }
}
