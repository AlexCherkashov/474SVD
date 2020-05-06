using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotCheapBook
{
    abstract class BookShop
    {
        public abstract Tuple<int, string> GetBestBook(string name);
        protected abstract  Tuple<int, string> FindBook(string name);

        protected string GetData(int i, string data, char symbol)
        {
            string bookID = "";
            while (data[i] != symbol)
            {
                bookID += data[i];
                i++;
            }
            return bookID.Replace(" ", "");
        }

        protected string GetBookName(string name, string symbol)
        {
            if (name == "") return "";
            var array = name.Split();
            string result = array[0];
            for (var i = 1; i < array.Length; i++)
            {
                result += symbol + array[i];
            }
            return result;
        }
    }
}
