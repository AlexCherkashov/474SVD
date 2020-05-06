using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BotCheapBook
{
    class BookSearch
    {
        public static void GetBook(WebClient webClient, string urlBotMsg, string id, string msg, SqlConnection sql)
        {

            Program.SendMessage(webClient, urlBotMsg, id, "Поиск...");

            SqlCommand command = new SqlCommand("SELECT * FROM [History] WHERE bookName = @bookName", sql);
            command.Parameters.AddWithValue("bookName", msg);
            SqlDataReader sqlReader = command.ExecuteReader();
            var data = new Tuple<int, string>(0, null);
            if (!sqlReader.HasRows)
            {
                data = Program.GetLink(msg);
                if (data != null)
                {
                    sqlReader.Close();
                    command = new SqlCommand("INSERT INTO [History] (bookName, url, price)" +
                                            "VALUES(@bookName, @url, @price)", sql);
                    command.Parameters.AddWithValue("bookName", msg);
                    command.Parameters.AddWithValue("url", data.Item2);
                    command.Parameters.AddWithValue("price", data.Item1);
                    command.ExecuteNonQuery();
                    sqlReader.Close();
                }
            }
            else
            {
                while (sqlReader.Read())
                {
                    string url = sqlReader[2].ToString();
                    string price = sqlReader[3].ToString();
                    data = new Tuple<int, string>(int.Parse(price), url);
                }
                sqlReader.Close();
            }

            if (data.Item1 == 0)
            {
                Program.SendMessage(webClient, urlBotMsg, id, "Прости, я ничего не нашёл");
            }
            else
            {
                Program.SendMessage(webClient, urlBotMsg, id, "Эту книгу можно купить за " + data.Item1 + " рублей");
                Program.SendMessage(webClient, urlBotMsg, id, data.Item2);
            }
        }
    }
}
