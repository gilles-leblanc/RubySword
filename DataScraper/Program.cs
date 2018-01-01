using DataScraper.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace DataScraper
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        private static HttpClient httpClient;

        static void Main(string[] args)
        {
            httpClient = new HttpClient();

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                    .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            IConfigurationSection monsterUrlsSection = Configuration.GetSection("monsterUrls");
            var monsterUrls = monsterUrlsSection.AsEnumerable();

            monsterUrls.Where(x => !string.IsNullOrWhiteSpace(x.Value))
                       //.Select(hub => DownloadPage(hub.Value))
                       //.SelectMany(html => GetAllLinks(html))
                       //.Where(link => IsMonsterPageLink(link))
                       //.Select(mob => DownloadPage(mob))
                       .Select(mob => DownloadPage("http://www.d20pfsrd.com/bestiary/monster-listings/constructs/clockwork/clockwork-reliquary/"))
                       .Where(html => IsValidMonster(html))
                       .Select(html => new Monster(html))
                       .ToList()
                       .ForEach(x => WriteToFile(x));
        }

        private static string DownloadPage(string url)
        {
            try
            {
                return httpClient.GetStringAsync(url).Result;
            }
            catch (Exception)
            {
                // if we can't download the page, ie: broken link, continue on with the other pages
                return string.Empty;
            }
        }

        private static IEnumerable<string> GetAllLinks(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            return document.DocumentNode.SelectNodes("//a[@href]").Select(a => a.GetAttributeValue("href", null))
                                                                  .Where(lnk => !string.IsNullOrWhiteSpace(lnk)); 
        }

        private static bool IsMonsterPageLink(string link) => link.Contains("/bestiary/monster-listings/");

        private static bool IsValidMonster(string html) => html.Contains("XP ") && html.Contains("Init ");

        private static void WriteToFile(Monster monster) => File.WriteAllText($"./output/{monster.Name}.json", monster.ToJson());
    }
}
