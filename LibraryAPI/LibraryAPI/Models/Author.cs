namespace LibraryAPI.Models
{
    public class Author
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
    }

    public class AuthorDto
    {
        public string Name { get; set; }
        public string Surname { get; set;}
    }
}
