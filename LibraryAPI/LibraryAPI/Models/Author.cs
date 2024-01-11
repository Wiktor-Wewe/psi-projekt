namespace LibraryAPI.Models
{
    public class Author
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Surname { get; set; }

        public List<Book> Books { get; } = new();
    }

    public class AuthorDto
    {
        public Guid? Id { get; set; }
        public required string Name { get; set; }
        public required string Surname { get; set;}
    }
}
