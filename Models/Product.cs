using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ReservedParser.Models
{
    internal class Product
    {
        [JsonIgnore]
        public List<string> Category { get; set; } = new List<string>();
        [JsonIgnore]
        public List<string> SubCategory { get; set; } = new List<string>();
        [JsonIgnore]
        public List<string> SubSubCategory { get; set; } = new List<string>();
        [JsonPropertyName("id")]
        public string product_id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public bool has_discount { get; set; }
        public string[] img { get; set; } = { string.Empty };
        public string price { get; set; } = string.Empty;
        public string final_price { get; set; } = string.Empty;
        public string brand { get; set; } = string.Empty;
        public string photoDescription { get; set; } = string.Empty;
        public string url { get; set; } = string.Empty;
        public Size[] sizes { get; set; } = new Size[] { };
    }
    internal class Size
    {
        public string sizeName { get; set; } = string.Empty;
        public bool stock { get; set; }
        public int megentoId { get; set; }
        public int sizeId { get; set; }
        public int sizeOrder { get; set; }
    }
}
