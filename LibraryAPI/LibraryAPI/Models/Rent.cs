namespace LibraryAPI.Models
{
    public class Rent
    {
        public Guid Id { get; set; }
        public DateOnly RentDate { get; set; }
        public DateOnly PlannedReturnDate { get; set; }
        public DateOnly? ReturnDate { get; set; }
        public Member Member { get; set; }
        public List<Book> Books { get; set; }
        public Employee Employee { get; set; }
    }

    public class RentDto
    {
        public DateOnly RentDate { get; set; }
        public DateOnly PlannedReturnDate { get; set; }
        public DateOnly? ReturnDate { get; set; }
        public Guid Member { get; set; }
        public List<Guid> Books { get; set; }
        public Guid Employee { get; set; }
    }
}
