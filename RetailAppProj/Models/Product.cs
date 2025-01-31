
using System;

namespace RetailApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public DateTime PostedDate { get; set; } = DateTime.UtcNow;
    }
}
