using DomainModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataConverter
{
    public class Program
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
            IConfigurationSection skillConversion = Configuration.GetSection("skillConversion");
            IConfigurationSection namesToSkip = Configuration.GetSection("namesToSkip");

            var skillConversionTable = new Dictionary<string, string>();
            skillConversion.Bind(skillConversionTable);

            var listNamesToSkip = new List<string>();
            namesToSkip.Bind(listNamesToSkip);

            // open each file
            foreach (string filePath in Directory.EnumerateFiles(inputDirectory.Value, "*.json"))
            {
                string value = File.ReadAllText(filePath);
                D20Monster d20monster = JsonConvert.DeserializeObject<D20Monster>(value);

                if (!listNamesToSkip.Contains(d20monster.Name))
                {
                    // convert and output
                    GenesysMonster genMonster = new GenesysMonster(d20monster, skillConversionTable);
                    WriteToFile(genMonster);
                }                
            }
        }

        private static void WriteToFile(GenesysMonster monster) => File.WriteAllText($"./output/{monster.Name}.json", monster.ToJson());
    }
}
