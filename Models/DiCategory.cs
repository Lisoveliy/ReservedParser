using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ReservedParser.Models
{
    internal class DiCategory
    {
        public string Id { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SystemName { get; set; } = string.Empty;
    }
}
