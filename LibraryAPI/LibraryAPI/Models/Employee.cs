namespace LibraryAPI.Models
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? JobPosition { get; set; }
    } 

    public class CreateEmployeeDto
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? JobPosition { get; set; }
    }
}
