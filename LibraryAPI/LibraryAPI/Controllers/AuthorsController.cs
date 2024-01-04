using LibraryAPI.Entities;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
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
                    Name = a.Name,
                    Surname = a.Surname
                }).AsQueryable(),
                page,
                pageSize
            );

            return Ok(paginatedAuthors);
        }

        [Authorize]
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
