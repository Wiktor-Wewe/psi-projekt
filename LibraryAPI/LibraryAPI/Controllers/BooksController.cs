using LibraryAPI.Entities;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LibraryAPI.Controllers
{
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
        public async Task<IActionResult> GetBooks(string? searchString, DateOnly? startDate, DateOnly? endDate, int page = 1, int pageSize = 10, string sortBy = "Title", bool ascending = true)
        {
            var query = _dbContext.Books
                .Include(x => x.Genres)
                .Include(x => x.Authors)
                .Include(x => x.PublishingHouse)
                .AsQueryable();

            if(sortBy == "Title")
            {
                if (searchString.IsNullOrEmpty() == false)
                {
                    query = query.Where(a => a.Title.Contains(searchString));
                }

                query = ascending ? query.OrderBy(a => a.Title) : query.OrderByDescending(a => a.Title);
            }
            else if (sortBy == "RelaseDate")
            {
                if (startDate.HasValue && endDate.HasValue)
                {
                    query = query.Where(r => r.RelaseDate > startDate && r.RelaseDate < endDate);
                }

                query = ascending ? query.OrderBy(a => a.RelaseDate) : query.OrderByDescending(a => a.RelaseDate);
            }
            else if (sortBy == "ISBN")
            {
                if (searchString.IsNullOrEmpty() == false)
                {
                    query = query.Where(a => a.ISBN.Contains(searchString));
                }

                query = ascending ? query.OrderBy(a => a.ISBN) : query.OrderByDescending(a => a.ISBN);
            }
            else
            {
                return BadRequest("Sorting Error");
            }

            var paginatedBooks = await PaginatedList<BookDto>.CreateAsync(
                query.Select(b => new BookDto
                {
                    Title = b.Title,
                    Description = b.Description,
                    RelaseDate = b.RelaseDate,
                    ISBN = b.ISBN,
                    Genres = b.Genres.Select(g => g.Id).ToList(),
                    Authors = b.Authors.Select(a => a.Id).ToList(),
                    PublishingHouse = b.PublishingHouseId

                }).AsQueryable(),
                page,
                pageSize
            );

            return Ok(paginatedBooks);
        }

        [Authorize]
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
