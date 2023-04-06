using ReservedParser.Models;
using System.Text.Json;
using GTranslatorAPI;

namespace ReservedParser
{
    internal class Program
    {
        public static Config Config = null!;
        public static GTranslatorAPIClient[] translatorThread = null!;
        private static DateTime lastTime;
        private static CancellationToken token;
        static void Main(string[] args)
        {
            token = new CancellationToken();
            lastTime = DateTime.Now;
            Config = Config.InitConfig("config.json");
            Console.WriteLine(Config.Host);
            MongoService.InitMongo(Config);
            var task = MainTask(Config);
            while (true)
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        if (DateTime.Now.Day != lastTime.Day)
                        {
                            task.Wait();
                            task = MainTask(Config);
                            lastTime = DateTime.Now;
                        }
                        Thread.Sleep(1000);
                    }
                });
                var key = Console.ReadKey();
                if(key.Key == ConsoleKey.Escape)
                {
                    return;
                }
            }
        }
        public static Task MainTask(Config config)
        {
            return Task.Run(() =>
            {
                var categories = CategoryParser.GetCategories(Config.Host);
                var dicategories = CategoryParser.GetInnerCategories(categories);
                var products = CategoryParser.DownloadProducts(dicategories);

                Console.WriteLine($"Products count: {products.Count}");
                Translator.TranslateProducts(products, Config);
                Console.WriteLine($"Adding to database...");
                MongoService.Drop();
                MongoService.products.InsertMany(products);
                Console.WriteLine("[DONE]");
            });
        }
    }
}