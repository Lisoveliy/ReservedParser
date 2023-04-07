using GTranslatorAPI;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ReservedParser.Models
{
    internal class Translator
    {
        private static int errors = 0;
        private static int warns = 0;
        public static void TranslateProducts(List<Product> products, Config config)
        {
            var ta = System.DateTime.Now;
            Console.Clear();
            errors = 0;
            warns = 0;
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
                        Console.WriteLine("Parsing {0}", x.url);
                        x.description = ParseDescription(x.url).Result;
                        x.description = Regex.Replace(x.description, "<.*?>", "");
                        x.description = x.description.Replace("&nbsp;", "");
                        await TranslateProduct(x, translatorThread[i/2]);
                        Thread.Sleep(100);
                    }
                }));
            }
            var taskConsole = Task.Run(() =>
            {
                Console.WriteLine();
                while (count < products.Count)
                {

                    var curpos = Console.GetCursorPosition();
                    if (curpos.Top > Console.WindowHeight - 2)
                    {
                        Console.Clear();
                        curpos.Top = 1;
                        curpos.Left = 0;
                    }
                    Console.SetCursorPosition(0, 0);
                    var timeelapsed = System.DateTime.Now - ta;
                    string hours = timeelapsed.Hours.ToString().Length < 2 ? "0" + timeelapsed.Hours.ToString() : timeelapsed.Hours.ToString();
                    string minutes = timeelapsed.Minutes.ToString().Length < 2 ? "0" + timeelapsed.Minutes.ToString() : timeelapsed.Minutes.ToString();
                    string seconds = timeelapsed.Seconds.ToString().Length < 2 ? "0" + timeelapsed.Seconds.ToString() : timeelapsed.Seconds.ToString();
                    Console.Write($"Translating {count}/{products.Count}... Please wait. TA: {hours}:{minutes}:{seconds} ");
                    var realcolor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Errors: {0}, Warnings: {1}", errors, warns);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.SetCursorPosition(0, curpos.Top);
                    Thread.Sleep(500);
                }
            });
            Tasks.ForEach(x =>
            {
                x.Wait();
            });
            taskConsole.Wait();
            Console.WriteLine("Errors {0}", errors);
            Console.WriteLine("Warnings {0}", warns);
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
                    var realcolor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Error ocurred while trying translate text. Retrying...{0}", e);
                    warns++;
                Console.ForegroundColor = ConsoleColor.White;
                    await TranslateProduct(x, translator);
                }
                catch (StackOverflowException)
                {
                    var realcolor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Too many failed attempts. Aborting translation...");
                    errors++;
                    Console.ForegroundColor = ConsoleColor.White;
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
                var realcolor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ignoring {url} description. Invalid parse data");
                errors++;
                Console.ForegroundColor = realcolor;
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
