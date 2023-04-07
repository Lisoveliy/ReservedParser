using GTranslatorAPI;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ReservedParser.Models
{
    internal class Translator
    {
        public static void TranslateProducts(List<Product> products, Config config)
        {
            var translatorThread = new List<GTranslatorAPIClient>();
            var Tasks = new List<Task>();
            int count = 0;
            for (int i = 0; i < config.TranslateThreads; i++)
            {
                translatorThread.Add(new GTranslatorAPIClient());
                Tasks.Add(Task.Run(async () =>
                {
                    while (count < products.Count)
                    {
                        var x = products[count++];
                        x.price = Convert.ToString(Convert.ToDouble(x.price) * config.PriceDef.PLNtoRUB + config.PriceDef.Comission + config.PriceDef.Delivery);
                        x.final_price = Convert.ToString(Convert.ToDouble(x.final_price) * config.PriceDef.PLNtoRUB + config.PriceDef.Comission + config.PriceDef.Delivery);
                        Console.WriteLine("Parsing {0}...", x.url);
                        x.description = ParseDescription(x.url).Result;
                        x.description = Regex.Replace(x.description, "<.*?>", " ");
                        x.description = x.description.Replace("&nbsp;", "");
                        await TranslateProduct(x, translatorThread[i/2]);
                        Thread.Sleep(100);
                    }
                }));
            }
            var taskConsole = Task.Run(() =>
            {
                while (count < products.Count)
                {
                    Console.WriteLine($"Translating {count}/{products.Count}... Please wait.");
                    Thread.Sleep(1000);
                }
            });
            Tasks.ForEach(x =>
            {
                x.Wait();
            });
            taskConsole.Wait();
        }
        static async Task TranslateProduct(Product x, GTranslatorAPIClient translator)
        {
            try
            {
                if(x.name != "")
                    x.name = (await translator.TranslateAsync("pl", "ru", x.name)).TranslatedText;
                List<string> desc = new List<string>();
                for(int i = 0; i < x.description.Length;)
                {
                    if (x.description.Length > 0)
                    {
                        int index = x.description.IndexOf(".", i) > x.description.IndexOf("\n", i) ? x.description.IndexOf(".", i) : x.description.IndexOf("\n", i);
                        if (i == index)
                        {
                            break;
                        }
                        desc.Add(x.description.Substring(i, index - i));
                        i = index;
                    }
                }
                x.description = "";
                foreach(var str in desc)
                {
                    if (str.Length < 1)
                    {
                        continue;
                    }
                    x.description += (await translator.TranslateAsync("pl", "ru", str)).TranslatedText;
                }
                //if(x.description != "")
                //Console.WriteLine(x.description);
                //x.description = (await translator.TranslateAsync("pl", "ru", x.description)).TranslatedText;

            }
            catch (Exception e)
            {
                if (e.Message.Contains("length"))
                {
                    return;
                }
                try
                {
                    Console.WriteLine("Error ocurred while trying translate text. Retrying...{0}", e.ToString());
                    await TranslateProduct(x, translator);
                }
                catch (StackOverflowException)
                {
                    Console.WriteLine("Too many failed attempts. Aborting translation...");
                    return;
                }
            }
        }
        static async Task<string> ParseDescription(string url)
        {
            try
            {
                //var url = "https://www.reserved.com/pl/pl/zakardowa-sukienka-0965c-mlc";
                string startstr = "return {\"brand\":\"";
                string endstr = "};";
                HttpClient client = new HttpClient();
                var resp = await client.GetAsync(url);
                var data = await resp.Content.ReadAsStringAsync();
                var jsonstr = data.Substring(data.LastIndexOf(startstr) + 7, data.IndexOf(endstr, data.LastIndexOf(startstr) + 7) - data.LastIndexOf(startstr));
                jsonstr = jsonstr.Substring(0, jsonstr.LastIndexOf("};") + 1);
                var des = JsonDocument.Parse(jsonstr).RootElement.GetProperty("description").GetString();
                return des!;
            }catch(Exception)
            {
                Console.WriteLine($"Ignoring {url} description. Invalid parse data");
                return "";
            }
        }
    }
    class TranslationOptions
    {
        [JsonPropertyName("q")]
        public string RequestString { get; set; } = string.Empty;
        public string source { get; set; } = string.Empty;
        public string target { get; set; } = string.Empty;
        public string format { get; set; } = string.Empty;
        public string api_key { get; set; } = "";
    }
}
