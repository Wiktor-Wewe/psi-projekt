using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public IActionResult GetMembers() 
        {
            var members = _dbContext.Members
                .Include(m => m.Rents)
                .ToList();

            var membersDto = members.Select(m => new MemberDto
            {
                Name = m.Name,
                Surname = m.Surname,
                Birthdate = m.Birthdate,
                Address = m.Address,
                PhoneNumber = m.PhoneNumber,
                Email = m.Email,
                Rents = m.Rents.Select(r => r.Id).ToList()
            }).ToList();

            return Ok(membersDto);
        }

        [HttpPost]
        public IActionResult CreateMember(MemberDto member)
        {
            var rents = _dbContext.Rents.Where(r => member.Rents.Contains(r.Id)).ToList();

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
            _dbContext.SaveChanges();

            return Ok(_dbContext.Members.FirstOrDefault(m => m.Surname == member.Surname && m.Email == member.Email));
        }
    }
}
