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
                    Id = b.Id,
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

        [HttpGet("{id}/publishingHouse")]
        public async Task<IActionResult> GetBooksPublishingHouse([FromRoute] Guid id)
        {
            var publishingHouse = await _dbContext.PublishingHouses
                .FirstOrDefaultAsync(ph => ph.Id == id);

            if(publishingHouse == null)
            {
                return NotFound("Publishing house not found");
            }

            var publishingHouseDto = new PublishingHouseDto
            {
                Id = publishingHouse.Id,
                Name = publishingHouse.Name,
                FoundationYear = publishingHouse.FoundationYear,
                Address = publishingHouse.Address,
                Website = publishingHouse.Website
            };

            return Ok(publishingHouseDto);
        }

        [HttpGet("{id}/Genres")]
        public async Task<IActionResult> GetBooksGenres([FromRoute] Guid id)
        {
            var book = await _dbContext.Books
                .Include(b => b.Genres)
                .FirstOrDefaultAsync(b => b.Id == id);

            if(book == null)
            {
                return NotFound("Book not found");
            }

            var genresDto = book.Genres.Select(g => new GenreDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description
            }).ToList();


            return Ok(genresDto);
        }

        [HttpGet("{id}/Authors")]
        public async Task<IActionResult> GetBooksAuthors([FromRoute] Guid id)
        {
            var book = await _dbContext.Books
                .Include(b => b.Authors)
                .FirstOrDefaultAsync(b => b.Id == id);

            if(book == null)
            {
                return NotFound("Book not found");
            }

            var authorsDto = book.Authors.Select(a => new AuthorDto
            {
                Id = a.Id,
                Name = a.Name,
                Surname = a.Surname
            }).ToList();

            return Ok(authorsDto);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateBook(BookDto book) 
        {
            var genres = await _dbContext.Genres.WhereAsync(g => book.Genres.Contains(g.Id)).ToList();
            var authors = await _dbContext.Authors.WhereAsync(a => book.Authors.Contains(a.Id)).ToList();
            var publishingHouse = await _dbContext.PublishingHouses.FirstOrDefaultAsync(p => p.Id == book.PublishingHouse);

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
            await _dbContext.SaveChangesAsync();

            var bookFromDb = await _dbContext.Books.FirstOrDefaultAsync(b => b.ISBN == book.ISBN);
            if (bookFromDb == null)
            {
                return NotFound();
            }

            var bookDto = new BookDto
            {
                Id = bookFromDb.Id,
                Title = bookFromDb.Title,
                Description = bookFromDb.Description,
                RelaseDate = bookFromDb.RelaseDate,
                ISBN = bookFromDb.ISBN,
                Genres = genres.select(x => x.Id).ToList(),
                Authors = authors.select(x => x.Id).ToList(),
                PublishingHouse = publishingHouse.Id
            };

            return Ok(bookDto); 
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook([FromRoute] Guid id)
        {
            var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            var bookDto = new BookDto
            {
                Id = book.id,
                Title = book.Title,
                Description = book.Description,
                RelaseDate = book.RelaseDate,
                ISBN = book.ISBN,
                Genres = book.Genres.select(x => x.id).toList(),
                Authors = book.Authors.select(x => x.id).toList(),
                PublishingHouse = PublishingHouse.id
            };

            return Ok(bookDto);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditBook([FromRoute] Guid id, BookDto book)
        {
            var originalBook = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (originalBook == null)
            {
                return NotFound();
            }

            var genre = await _dbContext.Genres.WhereAsync(g => book.Genres.Contains(g.Id)).ToList();

            var author = await _dbContext.Authors.WhereAsync(a => book.Authors.Contains(a.Id)).ToList();

            var publishinghouse = await _dbContext.PublishingHouses.FirstOrDefaultAsync(p => p.Id == id);
            if (publishinghouse == null)
            {
                return NotFound();
            }

            originalBook.ISBN = book.ISBN;
            originalBook.Title = book.Title;
            originalBook.Authors = author;
            originalBook.Genres = genre;
            originalBook.PublishingHouse = publishinghouse;
            if (book.Description != null)
            {
                originalBook.Description = book.Description;
            }
            if (book.RelaseDate != null)
            {
                originalBook.RelaseDate = book.RelaseDate;
            }

            _dbContext.SaveChangesAsync();

            var bookFromDb = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (bookFromDb == null)
            {
                return NotFound();
            }

            var bookDto = new BookDto
            {
                Id = bookFromDb.Id,
                Title = bookFromDb.Title,
                Description = bookFromDb.Description,
                RelaseDate = bookFromDb.RelaseDate,
                ISBN = bookFromDb.ISBN,
                Genres = genre.select(x => x.Id).ToList(),
                Authors = author.select(x => x.Id).ToList(),
                PublishingHouse = publishinghouse.Id,
            };

            return Ok(bookDto);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook([FromRoute] Guid id)
        {
            var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            _dbContext.Books.Remove(book);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
