using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace BotCheapBook
{
    class Respublica : BookShop
    {
        public override Tuple<int, string> GetBestBook(string name)
        {
            var priceAndBook = FindBook(GetBookName(name));
            if (priceAndBook == null) return null;
            string urlBook = "https://www.respublica.ru/knigi" + priceAndBook.Item2;
            return new Tuple<int, string>(priceAndBook.Item1, urlBook);
        }

        protected override Tuple<int, string> FindBook(string name)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string url = "https://www.respublica.ru/search?utf8=%E2%9C%93&%5Bquery%5D=" + name;
                string data = httpClient.GetStringAsync(url).Result;
                string findPrice = data.Substring(0, 12);
                string checkPrice = "item-price=";
                string checkBookID = "href=\"/knigi";
                List<string> prices = new List<string>();
                List<string> booksID = new List<string>();
                for (var i = 0; i < data.Length - 12; i++)
                {
                    if (checkPrice == findPrice.ToString().Substring(0, 11))
                        prices.Add(GetPrice(i + 12, data));
                    if (checkBookID == findPrice.ToString())
                    {
                        string thisBookID = GetBookId(i + 12, data);
                        if (!booksID.Contains(thisBookID))
                            booksID.Add(thisBookID);
                    }
                    findPrice = findPrice.Substring(1) + data[i + 12];
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
            return base.GetBookName(name, "+");
        }
    }
}
