using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryAPI.Controllers;
using LibraryAPI.Entities;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Tests.Tests
{
    [Category("RentsControllerTests")]
    public class RentsControllerTests
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
        public async Task GetAllRents_ShouldReturnAllRents_WithValidParameters()
        {
            // Arrange
            var context = await GetDatabaseContext();
            context.Rents.AddRange(new List<Rent>
            {
                new Rent { Id = Guid.NewGuid(), RentDate = new DateOnly(2022, 1, 1), PlannedReturnDate = new DateOnly(2022, 1, 10) },
                new Rent { Id = Guid.NewGuid(), RentDate = new DateOnly(2022, 2, 1), PlannedReturnDate = new DateOnly(2022, 2, 10) }
            });
            await context.SaveChangesAsync();

            var controller = new RentsController(context);

            // Act
            var result = await controller.GetAllRents(null, null, 1, 10, "RentDate", true) as OkObjectResult;
            var rents = result?.Value as PaginatedList<RentDto>;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(rents, Is.Not.Null);
            Assert.That(rents.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetRentsEmployee_ShouldReturnEmployee_WithValidParameters()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var employeeId = Guid.NewGuid();
            context.Employees.Add(new Employee { Id = employeeId, Name = "Alice", Surname = "Smith" });
            context.Rents.Add(new Rent { Id = Guid.NewGuid(), EmployeeId = employeeId });
            await context.SaveChangesAsync();

            var controller = new RentsController(context);

            // Act
            var result = await controller.GetRentsEmployee(employeeId) as OkObjectResult;
            var employeeDto = result?.Value as EmployeeDto;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(employeeDto, Is.Not.Null);
            Assert.That(employeeDto.Name, Is.EqualTo("Alice"));
            Assert.That(employeeDto.Surname, Is.EqualTo("Smith"));
        }

        [Test]
        public async Task GetRent_ShouldReturnRent_WithValidParameters()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var rentId = Guid.NewGuid();
            context.Rents.Add(new Rent { Id = rentId, RentDate = new DateOnly(2022, 1, 1), PlannedReturnDate = new DateOnly(2022, 1, 10) });
            await context.SaveChangesAsync();

            var controller = new RentsController(context);

            // Act
            var result = await controller.GetRent(rentId) as OkObjectResult;
            var rentDto = result?.Value as RentDto;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(rentDto, Is.Not.Null);
            Assert.That(rentDto.RentDate, Is.EqualTo(new DateOnly(2022, 1, 1)));
            Assert.That(rentDto.PlannedReturnDate, Is.EqualTo(new DateOnly(2022, 1, 10)));
        }

        [Test]
        public async Task CreateRent_ShouldCreateRent_WithValidData()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var controller = new RentsController(context);
            var member = new Member { Id = Guid.NewGuid(), Name = "John", Surname = "Doe" };
            var book1 = new Book { Id = Guid.NewGuid(), Title = "Book 1", ISBN = "123456" };
            var book2 = new Book { Id = Guid.NewGuid(), Title = "Book 2", ISBN = "789012" };
            var employee = new Employee { Id = Guid.NewGuid(), Name = "Alice", Surname = "Smith" };

            context.Members.Add(member);
            context.Books.AddRange(book1, book2);
            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            var rentDto = new RentDto
            {
                RentDate = new DateOnly(2022, 1, 1),
                PlannedReturnDate = new DateOnly(2022, 1, 10),
                Member = member.Id,
                Books = new List<Guid> { book1.Id, book2.Id },
                Employee = employee.Id
            };

            // Act
            var result = await controller.CreateRent(rentDto) as OkObjectResult;
            var createdRent = result?.Value as RentDto;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(createdRent, Is.Not.Null);
            Assert.That(createdRent.RentDate, Is.EqualTo(new DateOnly(2022, 1, 1)));
            Assert.That(createdRent.PlannedReturnDate, Is.EqualTo(new DateOnly(2022, 1, 10)));
        }

        [Test]
        public async Task EditRent_ShouldEditRent_WithValidData()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var controller = new RentsController(context);
            var originalRent = new Rent { Id = Guid.NewGuid(), RentDate = new DateOnly(2022, 1, 1), PlannedReturnDate = new DateOnly(2022, 1, 10) };
            context.Rents.Add(originalRent);
            await context.SaveChangesAsync();

            var member = new Member { Id = Guid.NewGuid(), Name = "John", Surname = "Doe" };
            var book = new Book { Id = Guid.NewGuid(), Title = "Book 1", ISBN = "123456" };
            var employee = new Employee { Id = Guid.NewGuid(), Name = "Alice", Surname = "Smith" };

            context.Members.Add(member);
            context.Books.Add(book);
            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            var rentDto = new RentDto
            {
                RentDate = new DateOnly(2022, 2, 1),
                PlannedReturnDate = new DateOnly(2022, 2, 10),
                Member = member.Id,
                Books = new List<Guid> { book.Id },
                Employee = employee.Id
            };

            // Act
            var result = await controller.EditRent(originalRent.Id, rentDto) as OkObjectResult;
            var editedRent = result?.Value as RentDto;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(editedRent, Is.Not.Null);
            Assert.That(editedRent.RentDate, Is.EqualTo(new DateOnly(2022, 2, 1)));
            Assert.That(editedRent.PlannedReturnDate, Is.EqualTo(new DateOnly(2022, 2, 10)));
        }

        [Test]
        public async Task DeleteRent_ShouldDeleteRent_WithValidData()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var controller = new RentsController(context);
            var rentId = Guid.NewGuid();
            context.Rents.Add(new Rent { Id = rentId });
            await context.SaveChangesAsync();

            // Act
            var result = await controller.DeleteRent(rentId) as OkResult;

            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}
