
using System;

namespace RetailApp.Models
{
    public class ApprovalQueue
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Action { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    }
}
