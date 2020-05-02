using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace BotCheapBook
{
    class Book24
    {
        public static Tuple<int, string> ByBook24(string name)
        {
            var priceAndBook = GetPrice(GetBookName(name));
            if (priceAndBook == null) return null;
            string urlBook = "https://book24.ru/product" + priceAndBook.Item2;
            return new Tuple<int, string>(priceAndBook.Item1, urlBook);
        }

        private static Tuple<int, string> GetPrice(string name)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string url = url = "https://book24.ru/search/?q=" + name + "&available=1";
                string data = httpClient.GetStringAsync(url).Result;
                string findPrice = data.Substring(0, 14);
                string checkPrice = "unitSalePrice";
                string checkBookID = "href=\"/product";
                List<string> prices = new List<string>();
                List<string> booksID = new List<string>();
                for (var i = 0; i < data.Length - 14; i++)
                {
                    if (checkPrice == findPrice.ToString().Substring(0, 13))
                        prices.Add(GetPrice(i + 15, data));
                    if (checkBookID == findPrice.ToString())
                    {
                        string thisBookID = GetBookId(i + 14, data);
                        if (!booksID.Contains(thisBookID))
                            booksID.Add(thisBookID);
                    }
                    findPrice = findPrice.Substring(1) + data[i + 14];

                }
                var list = new List<int>();
                foreach (var price in prices)
                {
                    Int32.TryParse(price, out int result);
                    if (result > 100)
                        list.Add(result);
                }
                list.Sort();
                int bestPrice = list.First();
                var bookID = booksID[prices.IndexOf(bestPrice.ToString())];
                return new Tuple<int, string>(bestPrice, bookID);
            }
            catch { return null; }
        }

        private static string GetBookId(int i, string data)
        {
            string bookID = "";
            while (data[i] != '\"')
            {
                bookID += data[i];
                i++;
            }
            return bookID.Replace(" ", "");
        }

        private static string GetPrice(int i, string data)
        {
            string price = "";
            while (data[i] != ',')
            {
                price += data[i];
                i++;
            }
            return price.Replace(" ", "");
        }

        private static string GetBookName(string name)
        {
            if (name == "") return "";
            var array = name.Split();
            string result = array[0];
            for (var i = 1; i < array.Length; i++)
            {
                result += "+" + array[i];
            }
            return result;
        }
    }
}
