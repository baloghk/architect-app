using System.ComponentModel.DataAnnotations;

namespace ArchitectApp.Models
{
    public class QuoteRequest
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required, StringLength(100)]
        public string ProjectType { get; set; }

        [Required, StringLength(100)]
        public string Location { get; set; }

        [Required]
        public string HousePlanPath { get; set; }

        [Required]
        public DateTime StartOfProject { get; set; }

        [Required]
        public DateTime EndOfProject { get; set; }

        [Required, StringLength(2000)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
