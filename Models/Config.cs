using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReservedParser.Models
{
    internal class Config
    {
        public string Host { get; set; } = "";
        public string TranlateHost { get; set; } = "";
        public string MongoString { get; set; } = "";
        public string MongoDB { get; set; } = "";
        public string MongoCollection { get; set; } = "";
        public int TranslateThreads { get; set; }
        public static Config InitConfig(string path)
        {
            FileStream file = File.OpenRead(path);
            return JsonSerializer.Deserialize<Config>(new StreamReader(file).ReadToEnd())!;
        }
    }
}
