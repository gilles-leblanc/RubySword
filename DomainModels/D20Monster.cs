using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DomainModels
{
    public class D20Monster
    {
        private const string rxNonDigits = @"[^\d]+";
        private const string rxHitDice = @"\d{1,2} {0,1}(d|D)";

        public string Name { get; set; }

        public List<string> Skills { get; set; }
        public List<string> Abilities { get; set; }

        private const string baseatk = "Atk";
        public int BaseAttackBonus { get; set; }

        private const string ac = "AC";
        public int Ac { get; set; }

        private const string hp = "hp";
        public int Hp { get; set; }

        public int HitDice { get; set; }

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

        public List<Attack> MeleeAttacks { get; set; }

        public List<Attack> RangedAttacks { get; set; }

        public List<Attack> AllAttacks
        {
            get
            {
                var attacks = new List<Attack>();
                attacks.AddRange(MeleeAttacks);
                attacks.AddRange(RangedAttacks);

                return attacks;
            }
        }
        
        public D20Monster()
        {
            // Empty constructor for deserialization
        }

        public D20Monster(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            Name = document.DocumentNode
                           .Descendants("h1")
                           .First()
                           .InnerText
                           .Trim();

            Name = string.Concat(Name.TakeWhile(c => c != '(')).TrimEnd();

            var statsBlock = GetBlock(document, StatsBlockSpecification());

            if (statsBlock == null)
                throw new InvalidOperationException($"Could not find stats block for {Name}.");

            Str = GetStat(statsBlock, str);
            Dex = GetStat(statsBlock, dex);
            Con = GetStat(statsBlock, con);
            Int = GetStat(statsBlock, @int);
            Wis = GetStat(statsBlock, wis);
            Cha = GetStat(statsBlock, cha);
                        
            var baseAttackBlock = GetBlock(document, BaseAttackBlockSpecification());

            if (baseAttackBlock != null)
                BaseAttackBonus = GetStat(baseAttackBlock, baseatk);

            Skills = GetSkills(document);
            Abilities = GetAbilities(document);

            var defensesBlock = GetBlock(document, DefensesBlockSpecification());

            if (defensesBlock == null)
                throw new InvalidOperationException($"Could not find defenses block for {Name}.");

            Ac = GetStat(defensesBlock, ac);
            Hp = GetStat(defensesBlock, hp);
            HitDice = GetHitDice(defensesBlock);
            Fortitude = GetStat(defensesBlock, fort);
            Reflex = GetStat(defensesBlock, @ref);
            Will = GetStat(defensesBlock, will);

            MeleeAttacks = Attack.GetMeleeAttacks(document);
            RangedAttacks = Attack.GetRangedAttacks(document);
        }

        private Func<HtmlNode, bool> StatsBlockSpecification() => p => p.InnerHtml.Contains("Str")
                                                                    && p.InnerHtml.Contains("Dex")
                                                                    && p.InnerHtml.Contains("Wis")
                                                                    && p.InnerHtml.Contains("Cha");

        private Func<HtmlNode, bool> BaseAttackBlockSpecification() => p => (p.InnerHtml.Contains("Base Atk")) ||
                                                                            (p.InnerHtml.Contains("CMB") && p.InnerHtml.Contains("CMD"));

        private Func<HtmlNode, bool> DefensesBlockSpecification() => p => p.InnerText.Contains("Fort") && p.InnerText.Contains("Ref") 
                                                                       && p.InnerText.Contains("Will");

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
            try
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

                string sanitizedCandidate = candidate.Replace('—', '0')
                                                     .Replace('–', '0')
                                                     .Replace('-', '0')
                                                     .Trim();

                // for the case where we have only a + sign, replace with a zero
                sanitizedCandidate = sanitizedCandidate == "+" ? "0" : sanitizedCandidate;

                // before parsing remove non digit characters from the string to prevent errors
                sanitizedCandidate = Regex.Replace(sanitizedCandidate, rxNonDigits, "");

                if (!int.TryParse(sanitizedCandidate, out int parsedValue))
                    throw new InvalidOperationException($"Could not parse '{sanitizedCandidate}' for stat '{stat}' in statsblock: {statsBlock}.");

                return Math.Abs(parsedValue);
            }
            catch
            {
                return 0;
            }
        }        

        private static bool IsStatDelimeter(char c) => c == ',' || c == '(' || c == ';';

        private static int GetHitDice(HtmlNode statsBlock)
        {
            string hpHdLine = statsBlock.Descendants("b")
                                        .FirstOrDefault(d => d.InnerHtml.Contains(hp))
                                        ?.NextSibling
                                        ?.InnerText
                                        ?.Replace("\n", "");

            if (string.IsNullOrWhiteSpace(hpHdLine) || !hpHdLine.ToLower().Contains("d"))
            {
                string innerText = statsBlock.InnerText;
                string delimeter = "hp ";

                hpHdLine = string.Concat(innerText.Substring(innerText.IndexOf(delimeter))
                                                  .TakeWhile(c => c != ')'));
            }
            
            var hitDiceMatch = Regex.Match(hpHdLine, rxHitDice);

            if (hitDiceMatch == null || !hitDiceMatch.Success)
                throw new InvalidOperationException($"Could not find any hit dice value in statsblock: {statsBlock}.");

            // remove the letter D in the sample and convert to int
             if (!int.TryParse(hitDiceMatch.Value.Replace("d", "").Replace("D", ""), out int result))
                throw new InvalidOperationException($"In {nameof(GetHitDice)} could not parse {hitDiceMatch.Value} into an integer value.");

            return result;
        }

        private static List<string> GetSkills(HtmlDocument document)
        {
            var skills = document.DocumentNode
                                 .SelectNodes("//a[@href]")
                                 .Where(a => a.GetAttributeValue("href", null)
                                              .ToLower()
                                              .Contains("/skills/")                                             // find all skills links
                                          && (a?.NextSibling?.InnerText?.Contains("+")).GetValueOrDefault()
                                          && !(a?.NextSibling?.InnerText?.Contains("+0")).GetValueOrDefault())  // check that the skill is positive
                                 .Select(a => a.InnerText)                                                      // read skill name
                                 .Where(a => a.ToLower() != "skills")                                           // filter out links to the skills section
                                 .Distinct()                                                            
                                 .OrderBy(a => a);

            return skills.ToList();
        }

        private static List<string> GetAbilities(HtmlDocument document)
        {
            var abilities = new List<string>();

            var abilityConversion = new Dictionary<string, string>();
            abilityConversion.Add("poison", "Poison");
            abilityConversion.Add("petrif", "Petrification");
            abilityConversion.Add("disease", "Disease");
            abilityConversion.Add("paralyz", "Paralyzation");
            abilityConversion.Add("breath", "Breath Weapon");
            abilityConversion.Add("energy drain", "Energy Drain");

            try
            {
                var potentialAbilities = document.DocumentNode
                                                 .Descendants("h4")
                                                 .ToList();

                potentialAbilities.ForEach(pa => abilityConversion.Keys
                                                                  .ToList()
                                                                  .ForEach(conv => 
                                                                  {
                                                                      if (pa.InnerText.Contains(conv))
                                                                          abilities.Add(abilityConversion[conv]);
                                                                  }));

            }
            catch (Exception)
            {
                // do not block monster creation when we can't read abilities since they are considered secondary
                // and not a valid reason to prevent monster creation
            }

            return abilities;
        }

        public string ToJson() => JsonConvert.SerializeObject(this);
    }
}
