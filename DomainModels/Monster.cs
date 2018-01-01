using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataScraper.Models
{
    public class Monster
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Languages { get; set; }

        public List<string> Skills { get; set; }

        private const string ac = "AC";
        public int Ac { get; set; }

        private const string hp = "hp";
        public int Hp { get; set; }

        private const string str = "Str";
        public int Str { get; set; }

        private const string dex = "Dex";
        public int Dex { get; set; }

        private const string con = "Con";
        public int Con { get; set; }

        private const string @int = "Int";
        public int Int { get; set; }

        private const string wis = "Wis";
        public int Wis { get; set; }

        private const string cha = "Cha";
        public int Cha { get; set; }

        private const string fort = "Fort";
        public int Fortitude { get; set; }

        private const string @ref = "Ref";
        public int Reflex { get; set; }

        private const string will = "Will";
        public int Will { get; set; }

        // TODO: attacks

        public Monster(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            Name = document.DocumentNode
                           .Descendants("h1")
                           .First()
                           .InnerText;

            var statsBlock = document.DocumentNode
                                     .Descendants("p")
                                     .FirstOrDefault(p => p.InnerHtml.Contains("Str "));

            if (statsBlock == null)
                throw new InvalidOperationException("Could not find stats block.");

            Str = GetStat(statsBlock, str);
            Dex = GetStat(statsBlock, dex);
            Con = GetStat(statsBlock, con);
            Int = GetStat(statsBlock, @int);
            Wis = GetStat(statsBlock, wis);
            Cha = GetStat(statsBlock, cha);

            var defensesBlock = document.DocumentNode
                                       .Descendants("p")
                                       .FirstOrDefault(p => p.InnerHtml.Contains("Fort "));

            Ac = GetStat(defensesBlock, ac);
            Hp = GetStat(defensesBlock, hp);
            Fortitude = GetStat(defensesBlock, fort);
            Reflex = GetStat(defensesBlock, @ref);
            Will = GetStat(defensesBlock, will);
        }

        private static int GetStat(HtmlNode statsBlock, string stat)
        {
            string candidate = string.Empty;
            string delimeter = stat + " ";

            var possibleValue = statsBlock.Descendants()
                                          .SingleOrDefault(d => d.Name == "b" && d.InnerHtml == delimeter)
                                          ?.NextSibling
                                          ?.InnerText
                                          .TakeWhile(c => c != ',' && c != '(');

            if (possibleValue != null)
                candidate = string.Concat(possibleValue).Replace('—', '0').Trim();
            else
            {
                string innerText = statsBlock.InnerText;
                candidate = string.Concat(innerText.Substring(innerText.IndexOf(delimeter))
                                                   .TakeWhile(c => c != ','))
                                  .Substring(delimeter.Length);
            }

            int parsedValue = 0;
            if (!int.TryParse(candidate, out parsedValue))
                throw new InvalidOperationException($"Could not parse '{possibleValue}' for stat '{stat}' in statsblock: {statsBlock}.");

            return parsedValue;
        }

        public string ToJson() => JsonConvert.SerializeObject(this);
    }
}
