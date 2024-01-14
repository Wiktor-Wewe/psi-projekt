using LibraryAPI.Entities;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LibraryAPI.Controllers
{
    [Route("api/Genres")]
    [ApiController]
    public class GenresController : Controller
    {
        public readonly AppDbContext _dbContext;
        
        public GenresController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult> GetGenres(string? searchString, int page = 1, int pageSize = 10, string sortBy = "Name", bool ascending = true) 
        {
            var query = _dbContext.Genres.AsQueryable();

            // Sorting
            if(sortBy == "Name")
            {
                if (searchString.IsNullOrEmpty() == false)
                {
                    query = query.Where(a => a.Name.Contains(searchString));
                }
                query = ascending ? query.OrderBy(a => a.Name) : query.OrderByDescending(a => a.Name);
            }
            else if(sortBy == "Description")
            {
                if (searchString.IsNullOrEmpty() == false)
                {
                    query = query.Where(a => a.Description.Contains(searchString));
                }
                query = ascending ? query.OrderBy(a => a.Description) : query.OrderByDescending(a => a.Description);
            }
            else
            {
                return BadRequest("Sorting Error");
            }


            var paginatedGenres = await PaginatedList<GenreDto>.CreateAsync(
                query.Select(g => new GenreDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description
                }).AsQueryable(),
                page,
                pageSize
            );

            return Ok(paginatedGenres);
        }

        [HttpGet("{id}/Books")]
        public async Task<IActionResult> GetGenresBooks([FromRoute] Guid id, string? searchString, DateOnly? startDate, DateOnly? endDate, int page = 1, int pageSize = 10, string sortBy = "Title", bool ascending = true)
        {
            var query = _dbContext.Books
                .Include(x => x.Genres)
                .Include(x => x.Authors)
                .Include(x => x.PublishingHouse)
                .Where(b => b.Genres.Any(g => g.Id == id))
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
        public async Task<ActionResult> CreateGenre(GenreDto genre) 
        {
            var newGenre = new Genre()
            {
                Name = genre.Name,
                Description = genre.Description
            };

            _dbContext.Genres.Add(newGenre);
            await _dbContext.SaveChangesAsync();

            var genreFromDb = await _dbContext.Genres.FirstOrDefaultAsync(g => g.Name == genre.Name && g.Description == genre.Description);
            if(genreFromDb == null)
            {
                return NotFound();
            }

            var genreDto = new GenreDto
            {
                Id = genreFromDb.Id,
                Name = genreFromDb.Name,
                Description = genreFromDb.Description,
            };

            return Ok(genreDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGenre([FromRoute] Guid id)
        {
            var genre = await _dbContext.Genres.FirstOrDefaultAsync(g => g.Id == id);
            if (genre == null)
            {
                return NotFound();
            }

            var genreDto = new GenreDto
            {
                Id = id,
                Name = genre.Name,
                Description = genre.Description,
            };

            return Ok(genreDto);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditGenre([FromRoute] Guid id, GenreDto genre)
        {
            var originalGenre = await _dbContext.Genres.FirstOrDefaultAsync(g => g.Id == id);
            if (originalGenre == null)
            {
                return NotFound();
            }

            originalGenre.Name = genre.Name;
            originalGenre.Description = genre.Description;

            await _dbContext.SaveChangesAsync();
            var genreFromDb = await _dbContext.Genres.FirstOrDefaultAsync(g => g.Id == id);
            if (genreFromDb == null)
            {
                return NotFound();
            }

            var genreDto = new GenreDto
            {
                Id = genreFromDb.Id,
                Name = genreFromDb.Name,
                Description = genreFromDb.Description,
            };

            return Ok(genreDto);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGenre([FromRoute] Guid id)
        {
            var genre = await _dbContext.Genres.FirstOrDefaultAsync(g => g.Id == id);
            if (genre == null)
            {
                return NotFound();
            }

            _dbContext.Genres.Remove(genre);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
