using System.ComponentModel.DataAnnotations;

namespace CollegeEventManager.Models
{
    public class RegistrationModel
    {
        [Key]
        public int RegistrationId { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Phone { get; set; }

        public int EventId { get; set; }

        public EventModel? Event { get; set; }
    }
}
