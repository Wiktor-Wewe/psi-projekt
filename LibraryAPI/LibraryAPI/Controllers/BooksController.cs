using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers
{
    [Authorize]
    [Route("api/Books")]
    [ApiController]
    public class BooksController : Controller
    {
        private readonly AppDbContext _dbContext;

        public BooksController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetBooks()
        {
            return Ok(_dbContext.Books.ToList());
        }

        [HttpPost]
        public IActionResult CreateBook(BookDto book) 
        {
            var genres = _dbContext.Genres.Where(g => book.Genres.Contains(g.Id)).ToList();
            var authors = _dbContext.Authors.Where(a => book.Authors.Contains(a.Id)).ToList();
            var publishingHouse = _dbContext.PublishingHouses.FirstOrDefault(p => p.Id == book.PublishingHouse);

            var newBook = new Book()
            {
                Title = book.Title,
                Description = book.Description,
                RelaseDate = book.RelaseDate,
                ISBN = book.ISBN,
                Genres = genres,
                Authors = authors,
                PublishingHouse = publishingHouse
            };

            _dbContext.Books.Add(newBook);
            _dbContext.SaveChanges();
            
            return Ok(_dbContext.Books.FirstOrDefault(b => b.ISBN == book.ISBN)); 
        }
    }
}
