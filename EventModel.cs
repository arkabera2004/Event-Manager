using System.ComponentModel.DataAnnotations;

namespace CollegeEventManager.Models
{
    public class EventModel
    {
         [Key]
        public int EventId { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string? Venue { get; set; }

        public string? PosterPath { get; set; }

        public string? Description { get; set; }

        public ICollection<RegistrationModel>? Registrations { get; set; }
    }
}
