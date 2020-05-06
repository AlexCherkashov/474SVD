using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace BotCheapBook
{
    class MyShop : BookShop
    {
        public override Tuple<int, string> GetBestBook(string name)
        {
            var priceAndBook = FindBook(GetBookName(name));
            if (priceAndBook == null) return null;
            string urlBook = "https://my-shop.ru/shop/product/" + priceAndBook.Item2 + ".html";
            return new Tuple<int, string>(priceAndBook.Item1, urlBook);
        }

        protected override Tuple<int, string> FindBook(string name)
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

        private string GetBookId(int i, string data)
        {
            return base.GetData(i, data, '"');
        }

        private string GetPrice(int i, string data)
        {
            return base.GetData(i, data, '"');
        }

        private string GetBookName(string name)
        {
            return base.GetBookName(name, "%20");
        }
    }
}
