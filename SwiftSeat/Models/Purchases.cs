using System.ComponentModel.DataAnnotations;

namespace SwiftSeat.Models
{
    public class Purchases
    {

        [Key]
        public int PurchaseId { get; set; }

        public int NumberTickets { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Required]
        public string CustomerEmail { get; set; }

        [Required]
        public string CustomerPhone { get; set; }

        [Required]
        public string CardNumber { get; set; }

        [Required]
        public string CardExpiry { get; set; }

        [Required]
        public string CardCVV { get; set; }

        public DateTime PurchaseDate { get; set; }

        // Foreign Key
        public int EventId { get; set; }

        // Navigation property
        public Shows Event { get; set; }
    }
}
