using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace BotCheapBook
{
    class MyShop
    {
        public static Tuple<int, string> ByMyShop(string name)
        {
            var priceAndBook = GetPrice(GetBookName(name));
            if (priceAndBook == null) return null;
            string urlBook = "https://my-shop.ru/shop/product/" + priceAndBook.Item2 + ".html";
            return new Tuple<int, string>(priceAndBook.Item1, urlBook);
        }

        private static Tuple<int, string> GetPrice(string name)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string url = "https://my-shop.ru/shop/search/a/sort/z/page/1.html?f14_39=0&f14_16=6&f14_6=" + name + "&t=0&next=1&kp=6&flags=21&menu_catid=0";
                string data = httpClient.GetStringAsync(url).Result;
                string findPrice = data.Substring(0, 8);
                string checkPrice = "\"price\":";
                string checkBookID = "\"id\":";
                List<string> prices = new List<string>();
                List<string> booksID = new List<string>();
                for (var i = 0; i < data.Length - 8; i++)
                {
                    if (checkPrice == findPrice.ToString())
                        prices.Add(GetPrice(i + 9, data));
                    if (checkBookID == findPrice.ToString().Substring(0, 5))
                    {
                        string thisBookID = GetBookId(i + 6, data);
                        if (!booksID.Contains(thisBookID))
                            booksID.Add(thisBookID);
                    }
                    findPrice = findPrice.Substring(1) + data[i + 8];

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
            while (data[i] != '"')
            {
                bookID += data[i];
                i++;
            }
            return bookID.Replace(" ", "");
        }

        private static string GetPrice(int i, string data)
        {
            string price = "";
            while (data[i] != '"')
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
                result += "%20" + array[i];
            }
            return result;
        }
    }
}
