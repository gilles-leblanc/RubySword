using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DomainModels
{
    public class GenesysMonster
    {
        public string Name { get; set; }

        public int Brawn { get; set; }

        public int Agility { get; set; }

        public int Intellect { get; set; }

        public int Cunning { get; set; }

        public int Willpower { get; set; }

        public int Presence { get; set; }

        public int Soak { get; set; }

        public int WoundThreshold { get; set; }

        public int StrainThreshold { get; set; }

        public int MeleeDefense { get; set; }

        public int RangedDefense { get; set; }

        public List<string> Talents { get; set; }

        public List<string> Abilities { get; set; }

        public List<string> Skills { get; set; }

        public List<string> Equipment { get; set; }


        public GenesysMonster()
        {
            // Empty constructor for deserialization
        }

        public GenesysMonster(D20Monster d20monster, Dictionary<string, string> skillConversionTable)
        {
            // Info
            Name = d20monster.Name
                             .Replace("Clockwork", "Mechanical")
                             .Replace("&#8217;", "'")
                             .Replace("Vegepygmy", "Vegetalpygmy");

            // Characteristics
            Agility = ConvertAbility(d20monster.Dex);
            Brawn = ConvertAbility((d20monster.Str + d20monster.Con) / 2);
            Cunning = ConvertAbility((d20monster.Int + d20monster.Wis + d20monster.Cha) / 3);
            Intellect = ConvertAbility(d20monster.Int);
            Presence = ConvertAbility(d20monster.Cha);
            Willpower = ConvertAbility(d20monster.Wis);

            // Derived
            Soak = Brawn + ConvertSoak(d20monster.Ac);
            WoundThreshold = 2 * Math.Max(2, d20monster.HitDice) + Brawn;
            StrainThreshold = 2 * Math.Max(2, d20monster.HitDice) + Willpower;
            MeleeDefense = ConvertDefenses(d20monster.Ac);
            RangedDefense = ConvertDefenses(d20monster.Ac);

            // Skills, talents, abilities, etc.
            Skills = ConvertSkills(d20monster.Skills, skillConversionTable);
            Abilities = d20monster.Abilities.Distinct().ToList();

            var attacks = d20monster.MeleeAttacks.Where(atk => !string.IsNullOrWhiteSpace(atk.Name))
                                                 .Select(atk => d20monster.BaseAttackBonus > 0 ? 
                                                                $"{atk.Name} ({Math.Min(Brawn, d20monster.BaseAttackBonus / 5)})" 
                                                                : $"{atk.Name} (0)").ToList();

            attacks.AddRange(d20monster.RangedAttacks.Where(atk => !string.IsNullOrWhiteSpace(atk.Name))
                                                     .Select(atk => d20monster.BaseAttackBonus > 0 ?
                                                                    $"{atk.Name} ({Math.Min(Agility, d20monster.BaseAttackBonus / 5)})"
                                                                    : $"{atk.Name} (0)").ToList());

            Equipment = attacks ?? new List<string>();
        }               

        private static int ConvertAbility(int input)
        {
            if (input == 0)
                return 0;

            if (input >= 1 && input <= 7)
                return 1;

            if (input >= 8 && input <= 11)
                return 2;

            if (input >= 12 && input <= 15)
                return 3;

            if (input >= 16 && input <= 20)
                return 4;

            if (input >= 21 && input <= 25)
                return 5;

            if (input > 25)
                return 6;

            throw new InvalidOperationException($"Non valid input {input} for ConvertAbility");
        }

        private static List<string> ConvertSkills(List<string> skills, Dictionary<string, string> skillConversionTable)
        {
            var convertedSkills = new List<string>();

            skills.Where(skill => skillConversionTable.ContainsKey(skill) 
                               && skillConversionTable[skill] != "0")   // 0, is the missing value when we can't convert a skill
                  .ToList()
                  .ForEach(skill => convertedSkills.Add(skillConversionTable[skill]));
                  
            return convertedSkills;
        }

        private static int ConvertSoak(int ac)
        {
            if (ac <= 13)
                return 0;

            if (ac >= 14 && ac <= 16)
                return 1;

            if (ac >= 17)
                return 2;

            throw new InvalidOperationException($"Invalid ac value {ac}.");
        }

        private static int ConvertDefenses(int ac)
        {
            if (ac <= 19)
                return 0;

            if (ac >= 20 && ac <= 22)
                return 1;

            if (ac >= 23)
                return 2;

            throw new InvalidOperationException($"Invalid ac value {ac}.");
        }

        public string ToJson() => JsonConvert.SerializeObject(this);
    }
}
