using GTranslatorAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
                        await TranslateProduct(x, translatorThread[i/2]);
                    }
                }));
            }
            var taskConsole = Task.Run(() =>
            {
                while (count < products.Count)
                {
                    Console.WriteLine($"{count}/{products.Count} translated");
                    Thread.Sleep(1000);
                }
            });
            taskConsole.Wait();
        }
        static async Task TranslateProduct(Product x, GTranslatorAPIClient translator)
        {
            try
            {
                x.name = (await translator.TranslateAsync("pl", "ru", x.name)).TranslatedText;
                x.photoDescription = (await translator.TranslateAsync("pl", "ru", x.photoDescription)).TranslatedText;

            }
            catch (Exception e)
            {
                try
                {
                    Console.WriteLine("Error ocuped while trying translate text. Retrying...\nInner Exception: {0}", e.Message);
                    await TranslateProduct(x, translator);
                }
                catch (StackOverflowException)
                {
                    Console.WriteLine("Too many failed attempts. Aborting translation...");
                    return;
                }
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
