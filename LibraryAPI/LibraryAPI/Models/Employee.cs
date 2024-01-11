using Microsoft.AspNetCore.Identity;

namespace LibraryAPI.Models
{
    public class Employee : IdentityUser
    {
        public Guid Id { get; set; } 
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public string? JobPosition { get; set; }
    } 

    public class EmployeeDto
    {
        public Guid? Id { get; set; }
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public string? JobPosition { get; set; }
        public required string Password { get; set; }
    }
}
