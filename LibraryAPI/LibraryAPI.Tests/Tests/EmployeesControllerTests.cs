using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryAPI.Controllers;
using LibraryAPI.Entities;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Tests.Tests
{
    [Category("EmployeesControllerTests")]
    public class EmployeesControllerTests
    {
        private async Task<AppDbContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("InMemoryDbToTest")
                .Options;
            var databaseContext = new AppDbContext(options);
            databaseContext.Database.EnsureCreated();

            return databaseContext;
        }

        [Test]
        public async Task GetEmployees_ShouldReturnEmployees_WithValidParameters()
        {
            // Arrange
            var context = await GetDatabaseContext();
            context.Employees.AddRange(new List<Employee>
            {
                new Employee { Id = Guid.NewGuid(), Name = "John", Surname = "Doe", JobPosition = "Developer", PasswordHash = "hash1" },
                new Employee { Id = Guid.NewGuid(), Name = "Jane", Surname = "Doe", JobPosition = "Manager", PasswordHash = "hash2" }
            });
            await context.SaveChangesAsync();

            var controller = new EmployeesController(context, Mock.Of<IConfiguration>(), new Mock<IPasswordHasher<Employee>>().Object);

            // Act
            var result = await controller.GetEmployees(null, 1, 10, "Surname", true) as OkObjectResult;
            var employees = result?.Value as PaginatedList<EmployeeDto>;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(employees, Is.Not.Null);
            Assert.That(employees.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task CreateEmployee_ShouldCreateEmployee_WithValidData()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Jwt:SecretKey", "supersecretkey"},
                    {"Jwt:ValidIssuer", "localhost"},
                    {"Jwt:ValidAudience", "localhost"}
                })
                .Build();

            var passwordHasherMock = new Mock<IPasswordHasher<Employee>>();
            passwordHasherMock.Setup(x => x.HashPassword(It.IsAny<Employee>(), It.IsAny<string>()))
                .Returns("hashedPassword");

            var controller = new EmployeesController(context, configuration, passwordHasherMock.Object);
            var employeeDto = new EmployeeDto
            {
                Name = "New",
                Surname = "Employee",
                JobPosition = "Developer",
                Password = "newpassword"
            };

            // Act
            var result = await controller.Register(employeeDto) as OkObjectResult;
            var createdEmployee = result?.Value as EmployeeDto;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(createdEmployee, Is.Not.Null);
            Assert.That(createdEmployee.Name, Is.EqualTo("New"));
            Assert.That(createdEmployee.Surname, Is.EqualTo("Employee"));
            Assert.That(createdEmployee.JobPosition, Is.EqualTo("Developer"));
        }
    }
}
