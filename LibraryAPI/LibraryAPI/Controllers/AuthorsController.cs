using LibraryAPI.Entities;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Net.Http.Headers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace LibraryAPI.Controllers
{
    [Route("api/Authors")]
    [ApiController]
    public class AuthorsController : Controller
    {
        private readonly AppDbContext _dbContext;

        public AuthorsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        [HttpGet]
        public async Task<IActionResult> GetAuthors(string? searchString, int page = 1, int pageSize = 10, string sortBy = "Surname", bool ascending = true) 
        {
            var query = _dbContext.Authors.AsQueryable();

            if (sortBy == "Name")
            {
                if(searchString.IsNullOrEmpty() == false)
                {
                    query = query.Where(a => a.Name.Contains(searchString));
                }

                query = ascending ? query.OrderBy(a => a.Name) : query.OrderByDescending(a => a.Name);
            }
            else if (sortBy == "Surname")
            {
                if(searchString.IsNullOrEmpty() == false)
                {
                    query = query.Where(a => a.Surname.Contains(searchString));
                }

                query = ascending ? query.OrderBy(a => a.Surname) : query.OrderByDescending(a => a.Surname);
            }
            else
            {
                return BadRequest("Sorting Error");
            }

            var paginatedAuthors = await PaginatedList<AuthorDto>.CreateAsync(
                query.Select(a => new AuthorDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Surname = a.Surname
                }).AsQueryable(),
                page,
                pageSize
            );

            return Ok(paginatedAuthors);
        }

        [HttpGet("{id}/Books")]
        public async Task<IActionResult> GetAuthorsBooks([FromRoute] Guid id, string? searchString, DateOnly? startDate, DateOnly? endDate, int page = 1, int pageSize = 10, string sortBy = "Title", bool ascending = true)
        {
            var query = _dbContext.Books
                .Where(book => book.Authors.Any(author => author.Id == id))
                .Include(book => book.Authors)
                .Include(book => book.Genres)
                .Include(book => book.PublishingHouse)
                .AsQueryable();

            if (sortBy == "Title")
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateAuthor(AuthorDto author) 
        {
            var newAuthor = new Author()
            {
                Name = author.Name,
                Surname = author.Surname
            };

            _dbContext.Authors.Add(newAuthor);
            await _dbContext.SaveChangesAsync();

            var authorFromDb = await _dbContext.Authors.FirstOrDefaultAsync(a => a.Name == author.Name && a.Surname == author.Surname);
            if (authorFromDb == null)
            {
                return NotFound();
            }

            var authorDto = new AuthorDto
            {
                Id = authorFromDb.Id,
                Name = authorFromDb.Name,
                Surname = authorFromDb.Surname,
            };

            return Ok(authorDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuthor([FromRoute] Guid id)
        {
            var author = await _dbContext.Authors.FirstOrDefaultAsync(a => a.Id == id);
            if (author == null)
            {
                return NotFound();
            }

            var authorDto = new AuthorDto
            {
                Id = id,
                Name = author.Name,
                Surname = author.Surname
            };

            return Ok(authorDto);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditAuthor([FromRoute] Guid id, AuthorDto author)
        {
            var originalAuthor = await _dbContext.Authors.FirstOrDefaultAsync(a => a.Id == id);
            if (originalAuthor == null)
            {
                return NotFound();
            }

            originalAuthor.Surname = author.Surname;
            originalAuthor.Name = author.Name;

            await _dbContext.SaveChangesAsync();

            var authorFromDb = await _dbContext.Authors.FirstOrDefaultAsync(a => a.Id == id);
            if (authorFromDb == null)
            {
                return NotFound();
            }

            var authorDto = new AuthorDto
            {
                Id = authorFromDb.Id,
                Name = authorFromDb.Name,
                Surname = authorFromDb.Surname
            };

            return Ok(authorDto);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor([FromRoute] Guid id)
        {
            var author = await _dbContext.Authors.FirstOrDefaultAsync(a => a.Id == id);
            if (author == null)
            {
                return NotFound();
            }

            _dbContext.Authors.Remove(author);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
