namespace LibraryAPI.Models
{
    public class Book
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateOnly? RelaseDate { get; set; }
        public required string ISBN { get; set; }
        public Guid PublishingHouseId { get; set; }
        public PublishingHouse PublishingHouse { get; set; } = null!;
        public List<Genre> Genres { get; set; } = new();
        public List<Author> Authors { get; set; } = new();
        public List<Rent> Rent { get; } = new();
    }

    public class BookDto
    {
        public Guid? Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateOnly? RelaseDate { get; set; }
        public required string ISBN { get; set; }
        public required List<Guid> Genres { get; set; }
        public required List<Guid> Authors { get; set; }
        public Guid PublishingHouse { get; set; }
    }
}
