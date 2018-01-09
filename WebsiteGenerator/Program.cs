using DomainModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebsiteGenerator
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                    .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            IConfigurationSection outputDirectory = Configuration.GetSection("outputDirectory");
            Directory.CreateDirectory(outputDirectory.Value);

            IConfigurationSection inputDirectory = Configuration.GetSection("inputDirectory");

            Dictionary<char, List<GenesysMonster>> monsters = ReadMonsters(inputDirectory);

            string alphaIndexLink = Configuration.GetSection("alphaIndexLink").Value;
            string alphaLinkTemplate = File.ReadAllText(Configuration.GetSection("alphaIndexPage").Value);

            string monsterPageLink = Configuration.GetSection("monsterPageLink").Value;
            string monsterPageTemplate = File.ReadAllText(Configuration.GetSection("monsterPage").Value);

            var alphaLinks = new List<string>();

            var regexAlphaNumeric = new Regex("[^a-zA-Z0-9]");

            monsters.Keys.OrderBy(k => k)
                         .ToList()
                         .ForEach(letter => GeneratePagesForLetter(letter, outputDirectory, monsters, alphaIndexLink, 
                                                                   alphaLinkTemplate, monsterPageLink, monsterPageTemplate, 
                                                                   alphaLinks, regexAlphaNumeric));
            
            string indexTemplate = File.ReadAllText(Configuration.GetSection("indexPage").Value);

            File.WriteAllText(outputDirectory.Value + "/index.html", 
                              indexTemplate.Replace("{alphaLinks}", string.Join("<br />\n", alphaLinks)));
        }

        private static Dictionary<char, List<GenesysMonster>> ReadMonsters(IConfigurationSection inputDirectory)
        {
            var dictionary = new Dictionary<char, List<GenesysMonster>>();

            foreach (string filePath in Directory.EnumerateFiles(inputDirectory.Value, "*.json"))
            {
                string value = File.ReadAllText(filePath);
                GenesysMonster genMonster = JsonConvert.DeserializeObject<GenesysMonster>(value);

                char firstLetter = genMonster.Name[0];
                firstLetter = firstLetter == '‎' ? '0' : firstLetter;       // temporary patch for weird error...

                if (dictionary.ContainsKey(firstLetter))
                    dictionary[firstLetter].Add(genMonster);
                else
                    dictionary.Add(firstLetter, new List<GenesysMonster> { genMonster });
            }

            return dictionary;
        }

        private static void GeneratePagesForLetter(char letter, IConfigurationSection outputDirectory, Dictionary<char, List<GenesysMonster>> monsters,
                                                   string alphaIndexLink, string alphaLinkTemplate, string monsterPageLink, string monsterPageTemplate,
                                                   List<string> alphaLinks, Regex regexAlphaNumeric)
        {
            alphaLinks.Add(string.Format(alphaIndexLink, letter));

            string directoryName = $"{outputDirectory.Value}/{letter}";
            Directory.CreateDirectory(directoryName);

            var monsterLinks = new List<string>();

            monsters[letter].ForEach(m =>
            {
                GenerateMonstersForLetter(letter, outputDirectory, monsterPageLink, monsterPageTemplate, 
                                          regexAlphaNumeric, m, monsterLinks);
            });

            File.WriteAllText($"{outputDirectory.Value}/index-{letter}.html",
                              alphaLinkTemplate.Replace("{letter}", letter.ToString())
                                               .Replace("{monsterPageLinks}", string.Join("<br />\n", monsterLinks)));
        }

        private static void GenerateMonstersForLetter(char letter, IConfigurationSection outputDirectory, 
                                                      string monsterPageLink, string monsterPageTemplate, Regex regexAlphaNumeric, 
                                                      GenesysMonster m, List<string> monsterLinks)
        {
            string name = regexAlphaNumeric.Replace(m.Name, "");
            monsterLinks.Add(string.Format(monsterPageLink, letter, name, m.Name));

            File.WriteAllText($"{outputDirectory.Value}/{letter}/monster-{name}.html",
                              monsterPageTemplate.Replace("{Name}", m.Name)
                                                 .Replace("{brawn}", m.Brawn.ToString())
                                                 .Replace("{agility}", m.Agility.ToString())
                                                 .Replace("{intellect}", m.Intellect.ToString())
                                                 .Replace("{cunning}", m.Cunning.ToString())
                                                 .Replace("{willpower}", m.Willpower.ToString())
                                                 .Replace("{presence}", m.Presence.ToString())                                                 
                                                 .Replace("{soak}", m.Soak.ToString())
                                                 .Replace("{wt}", m.WoundThreshold.ToString())
                                                 .Replace("{st}", m.StrainThreshold.ToString())
                                                 .Replace("{md}", m.MeleeDefense.ToString())
                                                 .Replace("{rd}", m.RangedDefense.ToString())
                                                 .Replace("{skills}", string.Join(", ", m.Skills.OrderBy(x => x)))
                                                 .Replace("{equipment}", string.Join(", ", m.Equipment.OrderBy(x => x)))
                                                 .Replace("{letter}", letter.ToString()));
        }
    }
}
