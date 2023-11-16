using LibraryAPI.Models;
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
            return Ok(_dbContext.PublishingHouses.ToList());
        }

        [HttpPost]
        public IActionResult CreatePublishingHouse(CreatePublishingHouseDto publishingHouse)
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
