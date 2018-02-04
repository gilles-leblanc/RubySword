using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DomainModels
{
    public class Attack
    {
        private static readonly string rxAttackDelimeter = @"(,)|((or|and) )/i";
        private static readonly string rxNonAlpha = @"[^a-zA-Z]";

        private const string melee = "Melee";
        private const string ranged = "Ranged";

        public bool Ranged { get; set; }

        public string Name { get; set; }

        public string OriginalText { get; set; }


        public Attack()
        {
            // Empty constructor for deserialization
        }

        public static List<Attack> GetMeleeAttacks(HtmlDocument document) => GetAttacks(document, melee, ranged: false);

        public static List<Attack> GetRangedAttacks(HtmlDocument document) => GetAttacks(document, ranged, ranged: true);

        private static List<Attack> GetAttacks(HtmlDocument document, string attackType, bool ranged)
        {
            var attacks = new List<Attack>();

            var possibleValues = document.DocumentNode
                                         .Descendants("b")
                                         .Where(b => b.InnerHtml.ToLower().Contains(attackType.ToLower()))
                                         .Select(b => b?.NextSibling?.InnerText)
                                         .Where(b => !string.IsNullOrWhiteSpace(b))
                                         .Distinct()
                                         .ToList();

            foreach (var possible in possibleValues)
            {
                string[] substrings = Regex.Split(possible, rxAttackDelimeter);

                substrings.Select(atk => string.Concat(atk.TakeWhile(c => c != '(')))
                          .Select(atk => Regex.Replace(atk, rxNonAlpha, ""))
                          .ToList()
                          .ForEach(atk => attacks.Add(new Attack
                          {
                              OriginalText = possible,
                              Ranged = ranged,
                              Name = atk,
                          }));
            }

            return attacks;
        }
    }
}
