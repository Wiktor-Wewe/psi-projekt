using LibraryAPI.Entities;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LibraryAPI.Controllers
{
    [Route("api/PublishingHouses")]
    [ApiController]
    public class PublishingHousesController : Controller
    {
        private readonly AppDbContext _dbContext;

        public PublishingHousesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetPublishingHouses(string? searchString, DateOnly? startDate, DateOnly? endDate, int page = 1, int pageSize = 10, string sortBy = "Name", bool ascending = true)
        {
            var query = _dbContext.PublishingHouses.AsQueryable();

            // Sorting
            if (sortBy == "Name")
            {
                if (searchString.IsNullOrEmpty() == false)
                {
                    query = query.Where(a => a.Name.Contains(searchString));
                }

                query = ascending ? query.OrderBy(a => a.Name) : query.OrderByDescending(a => a.Name);
            }
            else if (sortBy == "FoundationYear")
            {
                if (startDate.HasValue && endDate.HasValue)
                {
                    query = query.Where(r => r.FoundationYear > startDate.Value.Year && r.FoundationYear < endDate.Value.Year);
                }

                query = ascending ? query.OrderBy(a => a.FoundationYear) : query.OrderByDescending(a => a.FoundationYear);
            }
            else if (sortBy == "Address")
            {
                if (searchString.IsNullOrEmpty() == false)
                {
                    query = query.Where(a => a.Address.Contains(searchString));
                }

                query = ascending ? query.OrderBy(a => a.Address) : query.OrderByDescending(a => a.Address);
            }
            else if (sortBy == "Website")
            {
                if (searchString.IsNullOrEmpty() == false)
                {
                    query = query.Where(a => a.Website.Contains(searchString));
                }

                query = ascending ? query.OrderBy(a => a.Website) : query.OrderByDescending(a => a.Website);
            }
            else
            {
                return BadRequest("Sorting Error");
            }

            var paginatedPublishingHouses = await PaginatedList<PublishingHouseDto>.CreateAsync(
                query.Select(ph => new PublishingHouseDto
                {   Id = ph.Id,
                    Name = ph.Name,
                    FoundationYear = ph.FoundationYear,
                    Address = ph.Address,
                    Website = ph.Website
                }).AsQueryable(),
                page,
                pageSize
            );

            return Ok(paginatedPublishingHouses);
        }

        [HttpGet("{id}/Books")]
        public async Task<IActionResult> GetPublishingHousesBooks([FromRoute] Guid id, string? searchString, DateOnly? startDate, DateOnly? endDate, int page = 1, int pageSize = 10, string sortBy = "Title", bool ascending = true)
        {
            var query = _dbContext.Books
                .Include(x => x.Genres)
                .Include(x => x.Authors)
                .Include(x => x.PublishingHouse)
                .Where(x => x.PublishingHouseId == id)
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
        public async Task<IActionResult> CreatePublishingHouse(PublishingHouseDto publishingHouse)
        {
            var newPublishingHouse = new PublishingHouse()
            {
                Name = publishingHouse.Name,
                FoundationYear = publishingHouse.FoundationYear,
                Address = publishingHouse.Address,
                Website = publishingHouse.Website
            };

            _dbContext.PublishingHouses.Add(newPublishingHouse);
            await _dbContext.SaveChangesAsync();

            var publishingHouseFromDb = await _dbContext.PublishingHouses.FirstOrDefaultAsync(p => p.Name == publishingHouse.Name);
            if (publishingHouseFromDb == null)
            {
                return NotFound();
            }

            var publishingHouseDto = new PublishingHouseDto
            {
                Id = publishingHouseFromDb.Id,
                Name = publishingHouseFromDb.Name,
                FoundationYear = publishingHouseFromDb.FoundationYear,
                Address = publishingHouseFromDb.Address,
                Website = publishingHouseFromDb.Website
            };

            return Ok(publishingHouseDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPublishingHouse([FromRoute] Guid id)
        {
            var house = await _dbContext.PublishingHouses.FirstOrDefaultAsync(p => p.Id == id);
            if (house == null)
            {
                return NotFound();
            }

            var publishingHouseDto = new PublishingHouseDto
            {
                Id = house.Id,
                Name = house.Name,
                FoundationYear = house.FoundationYear,
                Address = house.Address,
                Website = house.Website
            };

            return Ok(publishingHouseDto);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditPublishingHouse([FromRoute] Guid id, PublishingHouseDto house)
        {
            var originalHouse = await _dbContext.PublishingHouses.FirstOrDefaultAsync(p => p.Id == id);
            if (originalHouse == null)
            {
                return NotFound();
            }

            originalHouse.Name = house.Name;
            originalHouse.FoundationYear = house.FoundationYear;
            originalHouse.Address = house.Address;
            originalHouse.Website = house.Website;

            await _dbContext.SaveChangesAsync();

            var publishingHouseFromDb = await _dbContext.PublishingHouses.FirstOrDefaultAsync(p => p.Id == id);
            if (publishingHouseFromDb == null)
            {
                return NotFound();
            }

            var publishingHouseDto = new PublishingHouseDto
            {
                Id = publishingHouseFromDb.Id,
                Name = publishingHouseFromDb.Name,
                FoundationYear = publishingHouseFromDb.FoundationYear,
                Address = publishingHouseFromDb.Address,
                Website = publishingHouseFromDb.Website
            };

            return Ok(publishingHouseDto);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePublishingHouse([FromRoute] Guid id)
        {
            var house = await _dbContext.PublishingHouses.FirstOrDefaultAsync(p => p.Id == id);
            if (house == null)
            {
                return NotFound();
            }

            _dbContext.PublishingHouses.Remove(house);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
