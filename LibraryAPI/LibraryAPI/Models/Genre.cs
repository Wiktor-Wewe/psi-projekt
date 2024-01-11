namespace LibraryAPI.Models
{
    public class Genre
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }

        public List<Book> Books { get; } = new();
    }

    public class GenreDto
    {
        public Guid? Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
