using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult GetPublishingHouses()
        {
            var publishingHouses = _dbContext.PublishingHouses.ToList();

            var publishingHousesDto = publishingHouses.Select(ph => new PublishingHouseDto
            {
                Name = ph.Name,
                FoundationYear = ph.FoundationYear,
                Address = ph.Address,
                Website = ph.Website

            }).ToList();

            return Ok(publishingHousesDto);
        }

        [Authorize]
        [HttpPost]
        public IActionResult CreatePublishingHouse(PublishingHouseDto publishingHouse)
        {
            var newPublishingHouse = new PublishingHouse()
            {
                Name = publishingHouse.Name,
                FoundationYear = publishingHouse.FoundationYear,
                Address = publishingHouse.Address,
                Website = publishingHouse.Website
            };

            _dbContext.PublishingHouses.Add(newPublishingHouse);
            _dbContext.SaveChanges();

            return Ok(_dbContext.PublishingHouses.FirstOrDefault(p => p.Name == publishingHouse.Name));
        }
    }
}
