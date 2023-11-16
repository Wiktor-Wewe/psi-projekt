namespace LibraryAPI.Models
{
    public class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateOnly? RelaseDate { get; set; }
        public string ISBN { get; set; }
        public List<Genre> Genres { get; set; }
        public List<Author> Authors { get; set; }
        public PublishingHouse PublishingHouse { get; set; }
    }

    public class BookDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateOnly? RelaseDate { get; set; }
        public string ISBN { get; set; }
        public List<Guid> Genres { get; set; }
        public List<Guid> Authors { get; set; }
        public Guid PublishingHouse { get; set; }
    }
}
