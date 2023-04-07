using ReservedParser.Models;
using System.Text.Json;

namespace ReservedParser
{
    internal static class CategoryParser
    {
        public static List<Category> GetCategories(string host)
        {
            var request = new HttpClient();
            var response = request.GetAsync(host);
            response.Wait();
            var stringans = response.Result.Content.ReadAsStringAsync();
            stringans.Wait();
            var res = stringans.Result;
            int indextreedata = res.IndexOf("{\"tree\":");
            int indexstoptreedata = res.IndexOf("}]}]}]};") + 7;
            string treedata = res.Substring(indextreedata, indexstoptreedata - indextreedata);
            var doc = JsonDocument.Parse(treedata);
            var list = doc.RootElement.GetProperty("tree").Deserialize<List<Category>>(new JsonSerializerOptions());
            var newlist = new List<Category>(list!);
            list!.ForEach(x =>
            {
                if (x.id.Length > 4 || !x.include_in_main_menu)
                {
                    newlist.Remove(x);
                }
            });
            return newlist;
        }
        public static List<Product> DownloadProducts(List<DiCategory> dicategories)
        {
            List<Product> products = new List<Product>();
            for (int i = 0; i < dicategories.Count; i++)
            {
                var category = dicategories[i];
                var client = new HttpClient();
                var resp = client.GetAsync("https://arch.reserved.com/api/1/category/" + category.Id + "/products");
                resp.Wait();
                var ans = resp.Result.Content.ReadAsStringAsync();
                ans.Wait();
                var doc = JsonDocument.Parse(ans.Result);
                var docs = doc.RootElement.GetProperty("products").Deserialize<List<Product>>();
                docs!.ForEach(x =>
                {
                    var product = products.FirstOrDefault(y =>
                    {
                        return y.product_id == x.product_id;
                    });
                    if (product != null)
                    {
                        //if (product.Tags.FirstOrDefault(x => x == category.SubCategory) == null)
                        product.SubCategory.Add(category.SubCategory);
                        product.SubSubCategory.Add(category.SystemName);
                    }
                    else
                    {
                        x.Category.Add(category.Category);
                        x.SubCategory.Add(category.SubCategory);
                        x.SubSubCategory.Add(category.SystemName);
                        products.Add(x);
                    }
                });
                var realcolor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{i + 1}/{dicategories.Count} Category {category.Name} downloaded! [{category.Category}]");
                Console.ForegroundColor = ConsoleColor.White;
            }
            return products;
        }
        public static List<DiCategory> GetInnerCategories(List<Category> categories)
        {
            List<DiCategory> list = new List<DiCategory>();
            for(int i = 0; i < categories.Count; i++)
            {
                var category = categories[i];
                for(int j = 0; j < category.children!.Count; j++)
                {
                    var subcategory = category.children[j];
                    for(int k = 0; k < subcategory.children!.Count; k++)
                    {
                        /*DEBUG*/
                        //if (list.Count >= 2)
                        //{
                        //    break;
                        //}

                        var child = subcategory.children[k];
                        list.Add(new()
                        {
                            Id = child.id,
                            Name = child.name,
                            SystemName = child.system_name,
                            SubCategory = subcategory.name,
                            Category = category.name
                        });
                    }
                }
            }
            return list;
        }
    }
}
