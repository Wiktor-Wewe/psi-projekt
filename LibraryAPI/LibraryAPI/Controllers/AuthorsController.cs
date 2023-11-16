using LibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult GetAuthors() 
        {
            return Ok(_dbContext.Authors.ToList());
        }

        [HttpPost]
        public IActionResult CreateAuthor(AuthorDto author) 
        {
            var newAuthor = new Author()
            {
                Name = author.Name,
                Surname = author.Surname
            };

            _dbContext.Authors.Add(newAuthor);
            _dbContext.SaveChanges();

            return Ok(_dbContext.Authors.FirstOrDefault(a => a.Name == author.Name && a.Surname == author.Surname));
        }

    }
}
