﻿using LibraryAPI.Entities;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace LibraryAPI.Controllers
{
    [Authorize]
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
                if (startDate.HasValue && endDate.HasValue)
                {
                    query = query.Where(r => r.RentDate > startDate && r.RentDate < endDate);
                }

                query = ascending ? query.OrderBy(r => r.RentDate) : query.OrderByDescending(r => r.RentDate);
            }
            else if (sortBy == "PlannedReturnDate")
            {
                if (startDate.HasValue && endDate.HasValue)
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
                    Id = r.Id,
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

        [HttpGet("{id}/Member")]
        public async Task<IActionResult> GetRentsMember([FromRoute] Guid id)
        {
            var member = await _dbContext.Members
                .FirstOrDefaultAsync(m => m.Rents.Any(r => r.Id == id));

            if(member == null)
            {
                return NotFound("Member not found");
            }

            var memberDto = new MemberDto
            {
                Id = member.Id,
                Name = member.Name,
                Surname = member.Surname,
                Birthdate = member.Birthdate,
                Address = member.Address,
                PhoneNumber = member.PhoneNumber,
                Email = member.Email,
                Rents = member.Rents.Select(r => r.Id).ToList()
            };

            return Ok(memberDto);
        }

        [HttpGet("{id}/Books")]
        public async Task<IActionResult> GetRentsBooks([FromRoute] Guid id)
        {
            var books = await _dbContext.Rents
                .Include(r => r.Books)
                .Where(r => r.Id == id)
                .Select(r => r.Books)
                .FirstAsync();

            var booksDto = books.Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Description = b.Description,
                RelaseDate = b.RelaseDate,
                ISBN = b.ISBN,
                Genres = b.Genres.Select(b => b.Id).ToList(),
                Authors = b.Authors.Select(b => b.Id).ToList(),
                PublishingHouse = b.PublishingHouseId
            }).ToList();

            return Ok(booksDto);
        }

        [HttpGet("{id}/Employee")]
        public async Task<IActionResult> GetRentsEmployee([FromRoute] Guid id)
        {
            var employee = await _dbContext.Rents
                .Include(r => r.Employee)
                .Where(r => r.EmployeeId == id)
                .Select(r => r.Employee)
                .FirstAsync();

            if(employee == null) 
            {
                return NotFound("Employee not found");
            }

            var employeeDto = new EmployeeDto
            {
                Id = employee.Id,
                Name = employee.Name,
                Surname = employee.Surname,
                JobPosition = employee.JobPosition,
                Password = "************"
            };

            return Ok(employeeDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRent([FromRoute] Guid id)
        {
            var rent = await _dbContext.Rents
                .Include(r => r.Books)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rent == null)
            {
                return NotFound();
            }

            var rentDto = new RentDto
            {
                Id = rent.Id,
                RentDate = rent.RentDate,
                PlannedReturnDate = rent.PlannedReturnDate,
                ReturnDate = rent.RentDate,
                Member = rent.MemberId,
                Books = rent.Books.Select(b => b.Id).ToList(),
                Employee = rent.EmployeeId
            };

            return Ok(rentDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRent(RentDto rent)
        {
            var member = await _dbContext.Members.FirstOrDefaultAsync(m => m.Id == rent.Member);
            if(member == null)
            {
                return NotFound("Member not found");
            }

            var books = await _dbContext.Books.Where(b => rent.Books.Contains(b.Id)).ToListAsync();

            var employee = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Id == rent.Employee);
            if(employee == null)
            {
                return NotFound("Employee not found");
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
            await _dbContext.SaveChangesAsync();

            var rentFromDb = await _dbContext.Rents
                .Include(r => r.Books)
                .FirstOrDefaultAsync(r => r.RentDate == rent.RentDate && r.PlannedReturnDate == rent.PlannedReturnDate);

            if (rentFromDb == null)
            {
                return NotFound();
            }

            var rentDto = new RentDto
            {
                Id = rentFromDb.Id,
                RentDate = rentFromDb.RentDate,
                PlannedReturnDate = rentFromDb.PlannedReturnDate,
                ReturnDate = rentFromDb.ReturnDate,
                Member = rentFromDb.MemberId,
                Books = rentFromDb.Books.Select(x => x.Id).ToList(),
                Employee = rentFromDb.EmployeeId
            };

            return Ok(rentDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditRent([FromRoute]Guid id, RentDto rent)
        {
            var originalRent = await _dbContext.Rents
                .Include(r => r.Books)
                .FirstOrDefaultAsync(r => r.Id == id);

            if(originalRent == null)
            {
                return NotFound("Rent not found");
            }

            var member = await _dbContext.Members.FirstOrDefaultAsync(m => m.Id == rent.Member);
            if(member == null)
            {
                return NotFound("Member not found");
            }

            var books = await _dbContext.Books.Where(b => rent.Books.Contains(b.Id)).ToListAsync();

            var employee = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Id == rent.Employee);
            if(employee == null)
            {
                return NotFound("Employee not found");
            }

            originalRent.RentDate = rent.RentDate;
            originalRent.PlannedReturnDate = rent.PlannedReturnDate;
            originalRent.ReturnDate = rent.ReturnDate;
            originalRent.Member = member;
            originalRent.Books = books;
            originalRent.Employee = employee;

            await _dbContext.SaveChangesAsync();

            var rentFromDb = await _dbContext.Rents
                .Include(r => r.Books)
                .FirstOrDefaultAsync(r => r.Id == id);

            if(rentFromDb == null)
            {
                return NotFound();
            }

            var rentDto = new RentDto
            {
                Id = rentFromDb.Id,
                RentDate = rentFromDb.RentDate,
                PlannedReturnDate = rentFromDb.PlannedReturnDate,
                ReturnDate = rentFromDb.ReturnDate,
                Member = rentFromDb.MemberId,
                Books = rentFromDb.Books.Select(b => b.Id).ToList(),
                Employee = rentFromDb.EmployeeId,
            };

            return Ok(rentDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRent([FromRoute]Guid id)
        {
            var rent = await _dbContext.Rents.FirstOrDefaultAsync(r => r.Id == id);
            if(rent == null)
            {
                return NotFound();
            }

            _dbContext.Rents.Remove(rent);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
