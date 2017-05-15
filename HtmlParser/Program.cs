using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Чтение и запись данных отраслей с сайта http://www.3klik.kz");
            ThreeClickKz.WriteAll();
            Console.WriteLine("Запись завершена.");
            Console.ReadLine();
        }
    }
}
