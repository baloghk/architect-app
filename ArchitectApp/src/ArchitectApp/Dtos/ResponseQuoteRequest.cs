using System.ComponentModel.DataAnnotations;

namespace ArchitectApp.Dto
{
    public class ResponseQuoteRequest
    {
        public Guid PublicId { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string ProjectType { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string HousePlanPath { get; set; } = string.Empty;
        public DateTime StartOfProject { get; set; }
        public DateTime EndOfProject { get; set; } 
        public string Description { get; set; } = string.Empty;
    }
}
