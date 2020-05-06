using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace BotCheapBook
{
    class Labirint : BookShop
    {
        public override Tuple<int, string> GetBestBook(string name)
        {
            var priceAndBook = FindBook(GetBookName(name));
            if (priceAndBook == null) return null;
            string urlBook = "https://www.labirint.ru/books/" + priceAndBook.Item2 + "/";
            return new Tuple<int, string>(priceAndBook.Item1, urlBook);
        }

        protected override Tuple<int, string> FindBook(string name)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string url = url = @"https://www.labirint.ru/search/" + name + "/?order=relevance&way=back&stype=0&paperbooks=1&otherbooks=1&available=1&preorder=1&wait=1&price_min=&price_max=";
                string data = httpClient.GetStringAsync(url).Result;
                string findPrice = data.Substring(0, 20);
                string checkPrice = "data-discount-price=";
                string checkBookID = "data-product-id=";
                List<string> prices = new List<string>();
                List<string> booksID = new List<string>();
                for (var i = 0; i < data.Length - 20; i++)
                {
                    if (checkPrice == findPrice.ToString())
                        prices.Add(GetData(i + 21, data));
                    if (checkBookID == findPrice.ToString().Substring(0, 16))
                        booksID.Add(GetData(i + 17, data));
                    findPrice = findPrice.Substring(1) + data[i + 20];

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

        private string GetData(int i, string data)
        {
            return base.GetData(i, data, '"');
        }

        private string GetBookName(string name)
        {
            return base.GetBookName(name, "%20");
        }
    }
}
