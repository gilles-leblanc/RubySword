using DomainModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;

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

            // open each file
            foreach (string filePath in Directory.EnumerateFiles(inputDirectory.Value, "*.json"))
            {
                string value = File.ReadAllText(filePath);
                D20Monster d20monster = JsonConvert.DeserializeObject<D20Monster>(value);

                // convert and output
                GenesysMonster genMonster = new GenesysMonster(d20monster);
                WriteToFile(genMonster);
            }
        }

        private static void WriteToFile(GenesysMonster monster) => File.WriteAllText($"./output/{monster.Name}.json", monster.ToJson());
    }
}
