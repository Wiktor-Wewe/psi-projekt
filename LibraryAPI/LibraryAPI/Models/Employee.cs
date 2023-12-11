using Microsoft.AspNetCore.Identity;

namespace LibraryAPI.Models
{
    public class Employee : IdentityUser
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? JobPosition { get; set; }
    } 

    public class EmployeeDto
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? JobPosition { get; set; }
        public string Password { get; set; }
    }
}
