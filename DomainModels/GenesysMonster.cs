﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DomainModels
{
    public class GenesysMonster
    {
        private const int baseValue = 3;
        private const int hardCap = 20;

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

        public GenesysMonster(PathfinderMonster d20monster, Dictionary<string, string> skillConversionTable)
        {
            // Info
            Name = d20monster.Name;

            // Characteristics
            Agility = ConvertAbility(d20monster.Dex);
            Brawn = ConvertAbility((d20monster.Str + d20monster.Con) / 2);
            Cunning = ConvertAbility((d20monster.Int + d20monster.Wis + d20monster.Cha) / 3);
            Intellect = ConvertAbility(d20monster.Int);
            Presence = ConvertAbility(d20monster.Cha);
            Willpower = ConvertAbility(d20monster.Wis);

            // Derived
            Soak = Brawn + Math.Min(d20monster.Fortitude, hardCap);
            WoundThreshold = Brawn + (d20monster.Hp / 10);
            StrainThreshold = baseValue + d20monster.Fortitude + d20monster.Will;
            MeleeDefense = d20monster.Ac / 20;
            RangedDefense = (d20monster.Ac / 20 + d20monster.Reflex / 10) / 2;

            // Skills, talents, abilities, etc.
            Skills = ConvertSkills(d20monster.Skills, skillConversionTable);
            Equipment = d20monster.AllAttacks.Select(atk => atk.Name).ToList();
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

        public string ToJson() => JsonConvert.SerializeObject(this);
    }
}
