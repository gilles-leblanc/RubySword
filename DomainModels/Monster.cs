using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataScraper.Models
{
    public class Monster
    {
        private const string rxNonDigits = @"[^\d]+";

        public string Name { get; set; }

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


        public Monster(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            Name = document.DocumentNode
                           .Descendants("h1")
                           .First()
                           .InnerText;

            var statsBlock = GetBlock(document, StatsBlockSpecification());

            if (statsBlock == null)
                throw new InvalidOperationException($"Could not find stats block for {Name}.");

            Str = GetStat(statsBlock, str);
            Dex = GetStat(statsBlock, dex);
            Con = GetStat(statsBlock, con);
            Int = GetStat(statsBlock, @int);
            Wis = GetStat(statsBlock, wis);
            Cha = GetStat(statsBlock, cha);

            Skills = GetSkills(document);

            var defensesBlock = GetBlock(document, DefensesBlockSpecification());

            if (defensesBlock == null)
                throw new InvalidOperationException($"Could not find defenses block for {Name}.");

            Ac = GetStat(defensesBlock, ac);
            Hp = GetStat(defensesBlock, hp);
            Fortitude = GetStat(defensesBlock, fort);
            Reflex = GetStat(defensesBlock, @ref);
            Will = GetStat(defensesBlock, will);
        }

        private Func<HtmlNode, bool> StatsBlockSpecification() => p => p.InnerHtml.Contains("Str ") || p.InnerHtml.Contains("Str<");

        private Func<HtmlNode, bool> DefensesBlockSpecification() => p => p.InnerHtml.Contains("Fort");

        private static HtmlNode GetBlock(HtmlDocument document, Func<HtmlNode, bool> blockSpecification)
        {

            // First try to get the block with the p element which is more specific
            // baring that try a less specific search which might return the whole html document.
            return document.DocumentNode
                           .Descendants("p")
                           .FirstOrDefault(p => blockSpecification(p))
                   ??
                   document.DocumentNode
                           .Descendants()
                           .FirstOrDefault(p => blockSpecification(p));
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

            if (string.IsNullOrWhiteSpace(candidate))
            {
                string innerText = statsBlock.InnerText;
                string delimeter = stat + " ";

                candidate = string.Concat(innerText.Substring(innerText.IndexOf(delimeter))
                                                   .TakeWhile(c => !IsStatDelimeter(c)))
                                  .Substring(delimeter.Length);
            }

            string sanitizedCandidate = Regex.Replace(candidate.Replace('—', '0')
                                                               .Replace('–', '0')
                                                               .Trim(),
                                                      rxNonDigits,
                                                      "");

            int parsedValue = 0;
            if (!int.TryParse(sanitizedCandidate, out parsedValue))
                throw new InvalidOperationException($"Could not parse '{sanitizedCandidate}' for stat '{stat}' in statsblock: {statsBlock}.");

            return Math.Abs(parsedValue);
        }        

        private static bool IsStatDelimeter(char c) => c == ',' || c == '(' || c == ';';

        private static List<string> GetSkills(HtmlDocument document)
        {
            var skills = document.DocumentNode
                                 .SelectNodes("//a[@href]")
                                 .Where(a => a.GetAttributeValue("href", null)
                                              .ToLower()
                                              .Contains("/skills/"))
                                 .Select(a => a.InnerText)
                                 .Where(a => a.ToLower() != "skills")
                                 .Distinct()
                                 .OrderBy(a => a);

            return skills.ToList();
        }

        public string ToJson() => JsonConvert.SerializeObject(this);
    }
}
