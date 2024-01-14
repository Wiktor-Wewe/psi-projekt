namespace LibraryAPI.Models
{
    public class Rent
    {
        public Guid Id { get; set; }
        public DateTime RentDate { get; set; }
        public DateTime PlannedReturnDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public Guid MemberId { get; set; }
        public Member Member { get; set; } = null!;
        public List<Book> Books { get; set; } = new();
        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
    }

    public class RentDto
    {
        public Guid? Id { get; set; }
        public DateTime RentDate { get; set; }
        public DateTime PlannedReturnDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public Guid Member { get; set; }
        public required List<Guid> Books { get; set; }
        public Guid Employee { get; set; }
    }
}
