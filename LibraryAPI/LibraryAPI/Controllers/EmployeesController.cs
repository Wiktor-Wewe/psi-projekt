using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryAPI.Controllers
{
    [Route("api/Employee")]
    [ApiController]
    public class EmployeesController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<Employee> _passwordHasher;

        public EmployeesController(AppDbContext dbContext, IConfiguration configuration, IPasswordHasher<Employee> passwordHasher)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public IActionResult GetEmployees()
        {
            return Ok(_dbContext.Employees.ToList());
        }

        [HttpPost("Login")]
        public IActionResult Login(EmployeeDto employee)
        {
            var user = _dbContext.Employees.FirstOrDefault(e => e.Name == employee.Name && e.Surname == employee.Surname);
            if (user == null)
            {
                return NotFound();
            }

            if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, employee.Password) == PasswordVerificationResult.Failed)
            {
                return Unauthorized("zle haslo");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]); // check if is not null
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, $"{user.Name}-{user.Surname}-{user.JobPosition}"),
                    new Claim(JwtRegisteredClaimNames.Iss, _configuration["Jwt:ValidIssuer"]), // check if is not null
                    new Claim(JwtRegisteredClaimNames.Aud, _configuration["Jwt:ValidAudience"]), // check if is not null
                    // Dodaj inne roszczenia...
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return Ok(tokenString);
        }

        [HttpPost("Register")]
        public IActionResult Register(EmployeeDto employee)
        {
            var newEmploye = new Employee()
            {
                Name = employee.Name,
                Surname = employee.Surname,
                JobPosition = employee.JobPosition,
            };

            newEmploye.PasswordHash = _passwordHasher.HashPassword(newEmploye, employee.Password);

            _dbContext.Employees.Add(newEmploye);
            _dbContext.SaveChanges();

            return Ok(_dbContext.Employees.FirstOrDefault(e => e.Name == employee.Name && e.Surname == employee.Surname));
        }
    }
}
