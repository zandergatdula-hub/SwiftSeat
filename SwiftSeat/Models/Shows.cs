using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

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

        public List<Shows>? Tickets { get; set; }
        public int ShowId { get; set; }
        public DateTime ShowDate { get; set; }
    }
}
