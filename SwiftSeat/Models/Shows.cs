using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftSeat.Models
{
    public class Shows
    {
        [Key]
        public int EventId { get; set; }

        public string? Title { get; set; }

        public string Description { get; set; } = string.Empty;

        public DateTime EventDate { get; set;}

        public string Venue { get; set; } = string.Empty;

        [NotMapped]
        [Display(Name = "Upload Photo")]
        public IFormFile? PhotoFile { get; set; }

        public string? PhotoFileName { get; set; }
    }
}
