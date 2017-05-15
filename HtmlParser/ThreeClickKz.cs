using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace HtmlParser
{
    static class ThreeClickKz
    {
        private static WebClient WClient;
        private static string mainUrl;
        static ThreeClickKz()
        {
            WClient = new WebClient { Proxy = null, Encoding = Encoding.UTF8 };
            mainUrl = "http://www.3klik.kz/";
        }
        public static List<string> GetAllIndustryLinks()
        {
            List<string> allIndustryLinks = new List<string>();
            var html = new HtmlDocument();
            html.LoadHtml(WClient.DownloadString(mainUrl));
            HtmlNodeCollection c = html.DocumentNode.SelectNodes("//div[@id=\"C_branches\"]//div[@class='infoBlock']//a");
            if (c != null)
            {
                HtmlNode n;
                for (int i = 1; i < c.Count; i += 2)
                {
                    n = c[i];
                    if (n.Attributes["href"] != null)
                    {
                        allIndustryLinks.Add(mainUrl + n.Attributes["href"].Value);
                    }
                }
            }
            return allIndustryLinks;
        }
        public static List<string> GetAllRegionLinks(string urlIndustry)
        {
            List<string> allRegionLinks = new List<string>();
            var html = new HtmlDocument();
            html.LoadHtml(WClient.DownloadString(urlIndustry));
            HtmlNodeCollection c = html.DocumentNode.SelectNodes("//div[@class='rContent positions']//li//a");
            if (c != null)
            {
                // Обрабатываем каждую страницу (парсим из нее выбранные данные)
                foreach (HtmlNode n in c)
                {
                    if (n.Attributes["href"] != null)
                    {
                        allRegionLinks.Add(mainUrl + n.Attributes["href"].Value);
                    }
                }
            }
            return allRegionLinks;
        }
        public static List<string> GetAllPageLinks(string urlRegion)
        {
            List<string> allPageLinks = new List<string>();
            var html = new HtmlDocument();
            html.LoadHtml(WClient.DownloadString(urlRegion));
            HtmlNodeCollection c = html.DocumentNode.SelectNodes("//span[@class='pLink']//a");
            if (c != null)
            {
                foreach (HtmlNode n in c)
                {
                    if (n.InnerText != null)
                    {
                        allPageLinks.Add(n.InnerText);
                    }
                }
            }
            string max = "0";
            if (allPageLinks.Count > 0)
            {
                max = allPageLinks[allPageLinks.Count - 1];
            }
            List<string> pages = new List<string>();
            pages.Add(urlRegion);
            if (int.Parse(max) > 1)
            {
                for (int i = 2; i <= int.Parse(max); i++)
                {
                    pages.Add(urlRegion.Replace(".htm", "-page" + i + ".htm"));
                }
            }
            return pages;
        }
        public static List<string> GetAllLinks(string urlPage)
        {
            List<string> allLinks = new List<string>();
            var html = new HtmlDocument();
            html.LoadHtml(WClient.DownloadString(urlPage));
            HtmlNodeCollection c = html.DocumentNode.SelectNodes("//div[@class='rContent positions']//h3//a");
            if (c != null)
            {
                foreach (HtmlNode n in c)
                {
                    if (n.Attributes["href"] != null)
                    {
                        allLinks.Add(mainUrl + n.Attributes["href"].Value);
                    }
                }
            }
            return allLinks;
        }
        public static string GetIndustryData(string url)
        {
            var html = new HtmlDocument();
            html.LoadHtml(WClient.DownloadString(url));

            string fullName = html.DocumentNode.SelectSingleNode("//span[@class='fn org']").InnerText;
            string city = html.DocumentNode.SelectSingleNode("//span[@class='locality']").InnerText;

            string address = city;
            if (html.DocumentNode.SelectSingleNode("//span[@class='street-address']") != null)
            {
                address = city + ", " +
                               html.DocumentNode.SelectSingleNode("//span[@class='street-address']").InnerText;
            }
            string tel = "";
            if (html.DocumentNode.SelectSingleNode("//span[@class='tel']") != null)
            {
                tel = " " + html.DocumentNode.SelectSingleNode("//span[@class='tel']").InnerText;
            }
            string headFullName = "";
            HtmlNodeCollection c = html.DocumentNode.SelectNodes("//ul[@class='list-check']//span");
            HtmlNodeCollection c1 = html.DocumentNode.SelectNodes("//ul[@class='list-check']//li");
            if (c1 != null)
            {
                // Обрабатываем каждую страницу (парсим из нее выбранные данные)
                foreach (HtmlNode n in c1)
                {
                    if (n.InnerText != null)
                    {
                        headFullName += n.InnerText + "; ";
                    }
                }
            }
            else if (c != null)
            {
                foreach (HtmlNode n in c)
                {
                    if (n.InnerText != null)
                    {
                        headFullName += n.InnerText + "; ";
                    }
                }
            }
            return fullName + "\t" + address + "\t" + tel + "\t" +
                   headFullName + "\t" + DateTime.Now + "\t" + DateTime.Now + "\n";
        }

        public static List<string> GetIndustryNames()
        {
            List<string> industry = new List<string>();
            var html = new HtmlDocument();
            html.LoadHtml(WClient.DownloadString(mainUrl));
            HtmlNodeCollection c = html.DocumentNode.SelectNodes("//div[@id=\"C_branches\"]//div[@class='infoBlock']//li//a");
            foreach (HtmlNode n in c)
            {
                if (!string.IsNullOrEmpty(n.InnerText) && n.InnerText != c[1].InnerText)
                {
                    industry.Add(n.InnerText);
                }
            }
            return industry;
        }
        public static List<string> GetRegionNames(string urlIndustry)
        {
            List<string> region = new List<string>();
            var html = new HtmlDocument();
            html.LoadHtml(WClient.DownloadString(urlIndustry));
            HtmlNodeCollection c = html.DocumentNode.SelectNodes("//div[@class='rContent positions']//li//a");
            if (c != null)
            {
                foreach (HtmlNode n in c)
                {
                    if (n.InnerText != null)
                    {
                        region.Add(n.InnerText);
                    }
                }
            }
            return region;
        }
        public static void WriteAll()
        {
            for (int i = 0; i < GetIndustryNames().Count; i++)
            {
                for (int j = 0; j < GetRegionNames(GetAllIndustryLinks()[i]).Count; j++)
                {
                    WriteResultToFile(i, j);
                }
            }
        }
        public static string CreateDirectory(string nameDirectory)
        {
            string path = @"D:\3klikKz";
            string subpath = @"" + nameDirectory;
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            dirInfo.CreateSubdirectory(subpath);
            return path + "\\" + subpath + "\\";
        }
        public static void WriteToFile(string path, string text, FileMode fileMode)
        {
            using (FileStream fstream = new FileStream(path, fileMode))
            {
                byte[] array = Encoding.Default.GetBytes(text);
                fstream.Write(array, 0, array.Length);
            }
        }
        public static void WriteResultToFile(int indexIndustry, int indexRegion)
        {
            string path = CreateDirectory(GetIndustryNames()[indexIndustry]) +
                          GetRegionNames(GetAllIndustryLinks()[indexIndustry])[indexRegion] + ".txt";
            string headers = "IndustryName\tAddress\tTelephone\tCategories\tData_add\tData_update\n";
            WriteToFile(path, headers, FileMode.OpenOrCreate); // запись в файл заголовок
            foreach (var page in GetAllPageLinks(GetAllRegionLinks(GetAllIndustryLinks()[indexIndustry])[indexRegion]))
            {
                foreach (var link in GetAllLinks(page))
                {
                    WriteToFile(path, GetIndustryData(link), FileMode.Append);// запись в файл данные отрасли
                }
            }
        }

    }
}