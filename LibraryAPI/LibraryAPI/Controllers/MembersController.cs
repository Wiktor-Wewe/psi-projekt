using LibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers
{
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
            return Ok(_dbContext.Members.ToList());
        }

        [HttpPost]
        public IActionResult CreateMember(CreateMemberDto member)
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
