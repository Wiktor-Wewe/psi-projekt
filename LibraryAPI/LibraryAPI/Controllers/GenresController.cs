using LibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;

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
        public ActionResult GetGenres() 
        {
            return Ok(_dbContext.Genres.ToList());
        }

        [HttpPost]
        public ActionResult CreateGenre(GenreDto genre) 
        {
            var newGenre = new Genre()
            {
                Name = genre.Name,
                Description = genre.Description
            };

            _dbContext.Genres.Add(newGenre);
            _dbContext.SaveChanges();

            return Ok(_dbContext.Genres.FirstOrDefault(g => g.Name == genre.Name && g.Description == genre.Description));
        }
    }
}
