using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ReservedParser.Models
{
    internal class Category
    {
        public string id { get; set; } = string.Empty;
        public int level { get; set; }
        public string name { get; set; } = string.Empty;
        public string system_name { get; set; } = string.Empty;
        public bool include_in_main_menu { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Category>? children { get; set; }
    }
}
