using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models
{
    public class PublishingHouse
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public int? FoundationYear { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
    }

    public class PublishingHouseDto
    {
        public Guid? Id { get; set; }
        public required string Name { get; set; }
        public int? FoundationYear { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
    }
}
