using LibraryAPI.Entities;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        public async Task<IActionResult> GetEmployees(string? searchString, int page = 1, int pageSize = 10, string sortBy = "Surname", bool ascending = true)
        {
            var query = _dbContext.Employees.AsQueryable();

            // Sorting
            if(sortBy == "Surname")
            {
                if (searchString.IsNullOrEmpty() == false)
                {
                    query = query.Where(a => a.Surname.Contains(searchString));
                }
                query = ascending ? query.OrderBy(a => a.Surname) : query.OrderByDescending(a => a.Surname);
            }
            else if(sortBy == "Name")
            {
                if (searchString.IsNullOrEmpty() == false)
                {
                    query = query.Where(a => a.Name.Contains(searchString));
                }

                query = ascending ? query.OrderBy(a => a.Name) : query.OrderByDescending(a => a.Name);
            }
            else if(sortBy == "JobPosition")
            {
                if (searchString.IsNullOrEmpty() == false)
                {
                    query = query.Where(a => a.JobPosition.Contains(searchString));
                }

                query = ascending ? query.OrderBy(a => a.JobPosition) : query.OrderByDescending(a => a.JobPosition);
            }
            else
            {
                return BadRequest("Sorting Error");
            }

            var paginatedEmployees = await PaginatedList<EmployeeDto>.CreateAsync(
                query.Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Surname = e.Surname,
                    JobPosition = e.JobPosition,
                    Password = "********"
                }).AsQueryable(),
                page,
                pageSize
            );

            return Ok(paginatedEmployees);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] EmployeeDto employee)
        {
            var user = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Name == employee.Name && e.Surname == employee.Surname);
            if (user == null)
            {
                return NotFound();
            }

            if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, employee.Password) == PasswordVerificationResult.Failed)
            {
                return Unauthorized("zle haslo");
            }

            var secretKey = _configuration["Jwt:SecretKey"];
            var validIssuer = _configuration["Jwt:ValidIssuer"];
            var validAudience = _configuration["Jwt:ValidAudience"];

            if(secretKey == null || validIssuer == null || validAudience == null)
            {
                throw new Exception("Unable to create JWT");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, $"{user.Name}-{user.Surname}-{user.JobPosition}"),
                    new Claim(JwtRegisteredClaimNames.Iss, validIssuer),
                    new Claim(JwtRegisteredClaimNames.Aud, validAudience),
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return Ok(tokenString);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(EmployeeDto employee)
        {
            var newEmployee = new Employee()
            {
                Name = employee.Name,
                Surname = employee.Surname,
                JobPosition = employee.JobPosition,
            };

            newEmployee.PasswordHash = _passwordHasher.HashPassword(newEmployee, employee.Password);

            _dbContext.Employees.Add(newEmployee);

            await _dbContext.SaveChangesAsync();

            var employeeFromDb = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Name == employee.Name && e.Surname == employee.Surname);
            if (employeeFromDb == null)
            {
                return NotFound();
            }

            var employeeDto = new EmployeeDto
            {
                Id = employeeFromDb.Id,
                Name = employeeFromDb.Name,
                Surname = employeeFromDb.Surname,
                JobPosition = employeeFromDb.JobPosition,
                Password = "*************"
            };

            return Ok(employeeDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployee([FromRoute] Guid id)
        {
            var employee = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            var employeeDto = new EmployeeDto
            {
                Id = employee.Id,
                Name = employee.Name,
                Surname = employee.Surname,
                JobPosition = employee.JobPosition,
                Password = "**********"
            };

            return Ok(employeeDto);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditEmployee([FromRoute] Guid id, EmployeeDto employee)
        {
            var originalEmployee = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (originalEmployee == null)
            {
                return NotFound();
            }

            originalEmployee.Name = employee.Name;
            originalEmployee.Surname = employee.Surname;
            originalEmployee.JobPosition = employee.JobPosition;
            originalEmployee.PasswordHash = _passwordHasher.HashPassword(originalEmployee, employee.Password);

            await _dbContext.SaveChangesAsync();

            var employeeFromDb = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if(employeeFromDb == null)
            {
                return NotFound();
            }

            var employeeDto = new EmployeeDto
            {
                Id = employeeFromDb.Id,
                Name = employeeFromDb.Name,
                Surname = employeeFromDb.Surname,
                JobPosition = employeeFromDb.JobPosition,
                Password = "***********"
            };
            
            return Ok(employeeDto);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee([FromRoute] Guid id)
        {
            var employee = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            _dbContext.Employees.Remove(employee);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
