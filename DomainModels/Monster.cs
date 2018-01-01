using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace DataScraper.Models
{
    public class Monster
    {
        private const string str = "Str";
        private const string dex = "Dex";
        private const string con = "Con";
        private const string @int = "Int";
        private const string wis = "Wis";
        private const string cha = "Cha";

        public string Name { get; set; }

        public int Ac { get; set; }

        public int Hp { get; set; }

        public int Str { get; set; }

        public int Dex { get; set; }

        public int Con { get; set; }

        public int Int { get; set; }

        public int Wis { get; set; }

        public int Cha { get; set; }

        public Monster(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var statsBlock = document.DocumentNode
                                     .Descendants("p")
                                     .FirstOrDefault(p => p.InnerHtml.Contains("<b>Str </b>"));

            if (statsBlock == null)
                throw new InvalidOperationException("Could not find stats block.");

            Str = GetStat(statsBlock, str);
            Dex = GetStat(statsBlock, dex);
            Con = GetStat(statsBlock, con);
            Int = GetStat(statsBlock, @int);
            Wis = GetStat(statsBlock, wis);
            Cha = GetStat(statsBlock, cha);
        }

        private static int GetStat(HtmlNode statsBlock, string stat)
        {
            var possibleValue = statsBlock.Descendants()
                                          .SingleOrDefault(d => d.Name == "b" && d.InnerHtml == stat + " ")
                                          ?.NextSibling
                                          ?.InnerText
                                          .Replace('—', '0')
                                          .Trim()
                                          .TrimEnd(',');

            int parsedValue = 0;
            if (!int.TryParse(possibleValue, out parsedValue))
                throw new InvalidOperationException($"Could not parse {possibleValue} for stat {stat} in statsblock {statsBlock}.");

            return parsedValue;
        }

        public string ToJson() => JsonConvert.SerializeObject(this);
    }
}
