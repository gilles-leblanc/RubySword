using DomainModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace NameListGenerator
{
    class Program
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
            var jsonBuilder = new StringBuilder();
            jsonBuilder.AppendLine("{");
            jsonBuilder.AppendLine("\"monsterNames\": [");

            // open each file
            foreach (string filePath in Directory.EnumerateFiles(inputDirectory.Value, "*.json"))
            {
                string value = File.ReadAllText(filePath);
                D20Monster d20monster = JsonConvert.DeserializeObject<D20Monster>(value);

                jsonBuilder.AppendLine($"\"{d20monster.Name}\",");
            }

            jsonBuilder.AppendLine("]");
            jsonBuilder.AppendLine("}");

            File.WriteAllText($"./output/out.json", jsonBuilder.ToString());
        }
    }
}
