using LibraryAPI.Entities;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace LibraryAPI.Controllers
{
    // [Authorize] -- popraw!!!!!!!!!!!!!!!!
    [Route("api/Rents")]
    [ApiController]
    public class RentsController : Controller
    {
        private readonly AppDbContext _dbContext;

        public RentsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRents(DateOnly? startDate, DateOnly? endDate, int page = 1, int pageSize = 10, string sortBy = "RentDate", bool ascending = true)
        {
            var query = _dbContext.Rents
                .Include(m => m.Member)
                .Include(b => b.Books)
                .Include(e => e.Employee)
                .AsQueryable();

            // Sorting
            if (sortBy == "RentDate")
            {
                if(startDate.HasValue && endDate.HasValue)
                {
                    query = query.Where(r => r.RentDate > startDate && r.RentDate < endDate);
                }

                query = ascending ? query.OrderBy(r => r.RentDate) : query.OrderByDescending(r => r.RentDate);
            }
            else if (sortBy == "PlannedReturnDate")
            {
                if(startDate.HasValue && endDate.HasValue)
                {
                    query = query.Where(r => r.PlannedReturnDate > startDate && r.PlannedReturnDate < endDate);
                }

                query = ascending ? query.OrderBy(r => r.PlannedReturnDate) : query.OrderByDescending(r => r.PlannedReturnDate);
            }
            else
            {
                return BadRequest("Sorting Error");
            }

            var paginatedRents = await PaginatedList<RentDto>.CreateAsync(
                query.Select(r => new RentDto
                {
                    RentDate = r.RentDate,
                    PlannedReturnDate = r.PlannedReturnDate,
                    ReturnDate = r.ReturnDate,
                    Member = r.Member.Id,
                    Books = r.Books.Select(b => b.Id).ToList(),
                    Employee = r.Employee.Id
                }).AsQueryable(),
                page,
                pageSize
            );

            return Ok(paginatedRents);
        }

        [HttpGet("{id}")]
        public IActionResult GetRent([FromRoute] Guid id)
        {
            var rent = _dbContext.Rents.FirstOrDefault(r => r.Id == id);
            if (rent == null)
            {
                return NotFound();
            }
            return Ok(rent);
        }

        [HttpPost]
        public IActionResult CreateRent(RentDto rent)
        {
            var member = _dbContext.Members.FirstOrDefault(m => m.Id == rent.Member);
            if(member == null)
            {
                return NotFound();
            }

            var books = _dbContext.Books.Where(b => rent.Books.Contains(b.Id)).ToList();

            var employee = _dbContext.Employees.FirstOrDefault(e => e.Id == rent.Employee);
            if(employee == null)
            {
                return NotFound();
            }

            var newRent = new Rent()
            {
                RentDate = rent.RentDate,
                PlannedReturnDate = rent.PlannedReturnDate,
                ReturnDate = rent.ReturnDate,
                Member = member,
                Books = books,
                Employee = employee
            };

            _dbContext.Rents.Add(newRent);
            _dbContext.SaveChanges();

            return Ok(_dbContext.Rents.FirstOrDefault(r => r.RentDate == rent.RentDate && r.PlannedReturnDate == rent.PlannedReturnDate));
        }

        [HttpPut("{id}")]
        public IActionResult EditRent([FromRoute]Guid id, RentDto rent)
        {
            var originalRent = _dbContext.Rents.FirstOrDefault(r => r.Id == id);
            if(originalRent == null)
            {
                return NotFound();
            }

            var member = _dbContext.Members.FirstOrDefault(m => m.Id == rent.Member);
            if(member == null)
            {
                return NotFound();
            }

            var books = _dbContext.Books.Where(b => rent.Books.Contains(b.Id)).ToList();

            var employee = _dbContext.Employees.FirstOrDefault(e => e.Id == rent.Employee);
            if(employee == null)
            {
                return NotFound();
            }

            originalRent.RentDate = rent.RentDate;
            originalRent.PlannedReturnDate = rent.PlannedReturnDate;
            originalRent.ReturnDate = rent.ReturnDate;
            originalRent.Member = member;
            originalRent.Books = books;
            originalRent.Employee = employee;

            _dbContext.SaveChanges();
            return Ok(_dbContext.Rents.FirstOrDefault(r => r.Id == id));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteRent([FromRoute]Guid id)
        {
            var rent = _dbContext.Rents.FirstOrDefault(r => r.Id == id);
            if(rent == null)
            {
                return NotFound();
            }

            _dbContext.Rents.Remove(rent);
            _dbContext.SaveChanges();
            return Ok();
        }
    }
}
