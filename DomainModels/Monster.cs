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

        private const string languages = "Languages";
        public List<string> Languages { get; set; }

        private const string skills = "Skills";
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
                                     .FirstOrDefault(p => p.InnerHtml.Contains("Str ") ||
                                                          p.InnerHtml.Contains("Str<"));

            if (statsBlock == null)
                throw new InvalidOperationException("Could not find stats block.");

            Str = GetStat(statsBlock, str);
            Dex = GetStat(statsBlock, dex);
            Con = GetStat(statsBlock, con);
            Int = GetStat(statsBlock, @int);
            Wis = GetStat(statsBlock, wis);
            Cha = GetStat(statsBlock, cha);

            //Languages = GetEntryList(statsBlock, languages);
            //Skills = GetEntryList(statsBlock, skills);

            var defensesBlock = document.DocumentNode
                                       .Descendants("p")
                                       .FirstOrDefault(p => p.InnerHtml.Contains("Fort"));

            if (defensesBlock == null)
                throw new InvalidOperationException("Could not find defenses block.");

            Ac = GetStat(defensesBlock, ac);
            Hp = GetStat(defensesBlock, hp);
            Fortitude = GetStat(defensesBlock, fort);
            Reflex = GetStat(defensesBlock, @ref);
            Will = GetStat(defensesBlock, will);
        }

        private static int GetStat(HtmlNode statsBlock, string stat)
        {
            string candidate = string.Empty;

            var possibleValue = statsBlock.Descendants("b")
                                          .FirstOrDefault(d => d.InnerHtml.Contains(stat))
                                          ?.NextSibling
                                          ?.InnerText
                                          .TakeWhile(c => !IsStatDelimeter(c));

            if (possibleValue != null)
                candidate = string.Concat(possibleValue);
            else
            {
                string innerText = statsBlock.InnerText;
                string delimeter = stat + " ";

                candidate = string.Concat(innerText.Substring(innerText.IndexOf(delimeter))
                                                   .TakeWhile(c => !IsStatDelimeter(c)))
                                  .Substring(delimeter.Length);
            }

            string sanitizedCandidate = candidate.Replace('—', '0')
                                                 .Replace('–', '0')
                                                 .Replace("+", string.Empty)
                                                 .Trim();

            int parsedValue = 0;
            if (!int.TryParse(sanitizedCandidate, out parsedValue))
                throw new InvalidOperationException($"Could not parse '{sanitizedCandidate}' for stat '{stat}' in statsblock: {statsBlock}.");

            return parsedValue;
        }

        private static bool IsStatDelimeter(char c) => c == ',' || c == '(' || c == ';';

        //private static List<string> GetEntryList(HtmlNode statsBlock, string stat)
        //{
        //    var possibleValue = statsBlock.Descendants()
        //                                  .SingleOrDefault(d => d.Name == "b" && d.InnerHtml.Contains(stat));
        //                                  //?.NextSibling
        //                                  //?.InnerText
        //                                  //.TakeWhile(c => !IsStatDelimeter(c));


        //    throw new NotImplementedException();
        //}

        public string ToJson() => JsonConvert.SerializeObject(this);
    }
}
