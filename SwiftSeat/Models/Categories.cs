using System.ComponentModel.DataAnnotations;


namespace SwiftSeat.Models
{
    public class Categories
    {   
        [Key]
        [Required]
        [Display(Name = "Category")]
        // Primary key
        public int Id { get; set; }
       
        public string Name { get; set; } = string.Empty;
    }
}

