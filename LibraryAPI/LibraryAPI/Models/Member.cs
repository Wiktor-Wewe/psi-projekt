namespace LibraryAPI.Models
{
    public class Member
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public DateOnly Birthdate { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public List<Rent> Rents { get; set; } = new();
    }

    public class MemberDto
    {
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public DateOnly Birthdate { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public required List<Guid> Rents { get; set; }
    }
}
