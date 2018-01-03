using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace DomainModels
{
    public class Attack
    {
        private const string melee = "Melee";
        private const string ranged = "Ranged";

        public bool Ranged { get; set; }

        public string Name { get; set; }

        public int AttackBonus { get; set; }

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
                                         .ToList();

            foreach (var candidate in possibleValues)
            {
                attacks.Add(new Attack
                {
                    OriginalText = candidate,
                    Ranged = ranged,
                });
            }

            return attacks;
        }
    }
}
