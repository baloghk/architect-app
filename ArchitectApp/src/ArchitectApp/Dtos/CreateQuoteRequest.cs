using System.ComponentModel.DataAnnotations;

namespace ArchitectApp.Dto
{
    public class CreateQuoteRequest
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required, StringLength(100)]
        public string ProjectType { get; set; }

        [Required, StringLength(100)]
        public string Location { get; set; }

        [Required]
        public IFormFile HousePlan { get; set; }

        [Required]
        public DateTime StartOfProject { get; set; }

        [Required]
        public DateTime EndOfProject { get; set; }

        [Required, StringLength(2000)]
        public string Description { get; set; }
    }
}
