using DomainModels;
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

            IConfigurationSection outputDirectory = Configuration.GetSection("outputDirectory");
            Directory.CreateDirectory(outputDirectory.Value);

            IConfigurationSection monsterUrlsSection = Configuration.GetSection("pathfinderMonsterUrls");
            var monsterUrls = monsterUrlsSection.AsEnumerable();

            var monstersHtml = 
                monsterUrls.Where(x => !string.IsNullOrWhiteSpace(x.Value))
                           //.Select(hub => DownloadPage(hub.Value))
                           //.SelectMany(html => GetAllLinks(html))
                           //.Where(link => IsMonsterPageLink(link))
                           //.Select(mob => DownloadPage(mob))
                           .Select(mob => DownloadPage("http://www.d20pfsrd.com/bestiary/monster-listings/aberrations/brethedan/"))
                           .Where(html => IsValidMonster(html));

            foreach (var monsterHtml in monstersHtml)
            {
                try
                {
                    WriteToFile(new D20Monster(monsterHtml));
                }
                catch (Exception exception)
                {
                    File.AppendAllText("./output/errors.log", 
                                       exception.Message + "\n\n\n\n" +
                                       monsterHtml + "\n\n*********************************************************************\n\n");
                }
            }
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

        private static bool IsValidMonster(string html) => html.Contains("XP") && html.Contains("Init");

        private static void WriteToFile(D20Monster monster) => File.WriteAllText($"./output/{monster.Name}.json", monster.ToJson());
    }
}
