using LibraryAPI.Entities;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LibraryAPI.Controllers
{
    [Authorize]
    [Route("api/Members")]
    [ApiController]
    public class MembersController : Controller
    {
        private readonly AppDbContext _dbContext;

        public MembersController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetMembers(string? searchString, DateOnly? startDate, DateOnly? endDate, int page = 1, int pageSize = 10, string sortBy = "Surname", bool ascending = true) 
        {
            var query = _dbContext.Members
                .Include(m => m.Rents)
                .AsQueryable();

            // Sorting
            if (sortBy == "Name")
            {
                if (searchString.IsNullOrEmpty() == false)
                {
                    query = query.Where(a => a.Name.Contains(searchString));
                }

                query = ascending ? query.OrderBy(a => a.Name) : query.OrderByDescending(a => a.Name);
            }
            else if (sortBy == "Surname")
            {
                if (searchString.IsNullOrEmpty() == false)
                {
                    query = query.Where(a => a.Surname.Contains(searchString));
                }

                query = ascending ? query.OrderBy(a => a.Surname) : query.OrderByDescending(a => a.Surname);
            }
            else if (sortBy == "Birthdate")
            {
                if (startDate.HasValue && endDate.HasValue)
                {
                    query = query.Where(r => r.Birthdate > startDate && r.Birthdate < endDate);
                }

                query = ascending ? query.OrderBy(a => a.Birthdate) : query.OrderByDescending(a => a.Birthdate);
            }
            else if (sortBy == "Address")
            {
                if (searchString.IsNullOrEmpty() == false)
                {
                    query = query.Where(a => a.Address.Contains(searchString));
                }

                query = ascending ? query.OrderBy(a => a.Address) : query.OrderByDescending(a => a.Address);
            }
            else if (sortBy == "PhoneNumber")
            {
                if (searchString.IsNullOrEmpty() == false)
                {
                    query = query.Where(a => a.PhoneNumber != null && a.PhoneNumber.Contains(searchString));
                }

                query = ascending ? query.OrderBy(a => a.PhoneNumber) : query.OrderByDescending(a => a.PhoneNumber);
            }
            else if (sortBy == "Email")
            {
                if (searchString.IsNullOrEmpty() == false)
                {
                    query = query.Where(a => a.Email != null && a.Email.Contains(searchString));
                }

                query = ascending ? query.OrderBy(a => a.Email) : query.OrderByDescending(a => a.Email);
            }
            else
            {
                return BadRequest("Sorting Error");
            }

            var paginatedMembers = await PaginatedList<MemberDto>.CreateAsync(
                query.Select(m => new MemberDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Surname = m.Surname,
                    Birthdate = m.Birthdate,
                    Address = m.Address,
                    PhoneNumber = m.PhoneNumber,
                    Email = m.Email,
                    Rents = m.Rents.Select(r => r.Id).ToList()
                }).AsQueryable(),
                page,
                pageSize
            );

            return Ok(paginatedMembers);
        }

        [HttpGet("{id}/Rents")]
        public async Task<IActionResult> GetMembersRents([FromRoute] Guid id, DateOnly? startDate, DateOnly? endDate, int page = 1, int pageSize = 10, string sortBy = "RentDate", bool ascending = true)
        {
            var query = _dbContext.Rents
                .Include(m => m.Member)
                .Include(b => b.Books)
                .Include(e => e.Employee)
                .Where(r => r.MemberId == id)
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

        [HttpPost]
        public async Task<IActionResult> CreateMember(MemberDto member)
        {
            var rents = await _dbContext.Rents.WhereAsync(r => member.Rents.Contains(r.Id)).ToList();

            var newMember = new Member()
            {
                Name = member.Name,
                Surname = member.Surname,
                Birthdate = member.Birthdate,
                Address = member.Address,
                PhoneNumber = member.PhoneNumber,
                Email = member.Email,
                Rents = rents
            };

            _dbContext.Members.Add(newMember);
            await _dbContext.SaveChangesAsync();

            var memberFromDb = await _dbContext.Members.FirstOrDefault(m => m.Surname == member.Surname && m.Email == member.Email);
            if (memberFromDb == null)
            {
                return NotFound();
            }

            var memberDto = new MemberDto
            {
                Id = memberFromDb.Id,
                Name = memberFromDb.Name,
                Surname = memberFromDb.Surname,
                Birthdate = memberFromDb.Birthdate,
                Address = memberFromDb.Address,
                PhoneNumber = memberFromDb.PhoneNumber,
                Email = memberFromDb.Email,
                Rents = rents.select(x => x.Id).ToList()
            };

            return Ok(memberDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMember([FromRoute] Guid id)
        {
            var member = _dbContext.Members.FirstOrDefault(m => m.Id == id);
            if (member == null)
            {
                return NotFound();
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
                Rents = member.Rents.select(x => x.Id).ToList()
            };

            return Ok(memberDto);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditMember([FromRoute] Guid id, MemberDto member)
        {
            var originalMember = await _dbContext.Members.FirstOrDefaultAsync(m => m.Id == id);
            if (originalMember == null)
            {
                return NotFound();
            }

            var rent = await _dbContext.Rents.WhereAsync(r => member.Rents.Contains(r.Id)).ToList();

            originalMember.Name = member.Name;
            originalMember.Surname = member.Surname;
            originalMember.Email = member.Email;
            originalMember.Birthdate = member.Birthdate;
            originalMember.Address = member.Address;
            originalMember.PhoneNumber = member.PhoneNumber;
            originalMember.Rents = rent;

            await _dbContext.SaveChangesAsync();

            var memberFromDb = await _dbContext.Members.FirstOrDefaultAsync(m => m.Id == id);
            if (memberFromDb == null)
            {
                return NotFound();
            }

            var memberDto = new MemberDto
            {
                Id = memberFromDb.Id,
                Name = memberFromDb.Name,
                Surname = memberFromDb.Surname,
                Birthdate = memberFromDb.Birthdate,
                Address = memberFromDb.Address,
                PhoneNumber = memberFromDb.PhoneNumber,
                Email = memberFromDb.Email,
                Rents = memberDto.Rents.Select(r => r.Id).ToArray()
            };

            return Ok(memberDto);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember([FromRoute] Guid id)
        {
            var member = await _dbContext.Members.FirstOrDefaultAsync(m => m.Id == id);
            if (member == null)
            {
                return NotFound();
            }

            _dbContext.Members.Remove(member);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
