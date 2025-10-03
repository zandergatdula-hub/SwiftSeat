using System.ComponentModel.DataAnnotations;

namespace SwiftSeat.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }  

        [Required, StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Email { get; set; } = string.Empty;
    }
}
