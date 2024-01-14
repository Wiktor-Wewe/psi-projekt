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
    [Category("MembersControllerTests")]
    public class MembersControllerTests
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
        public async Task GetMembers_ShouldReturnMembers_WithValidParameters()
        {
            // Arrange
            var context = await GetDatabaseContext();
            context.Members.AddRange(new List<Member>
            {
                new Member { Id = Guid.NewGuid(), Name = "John", Surname = "Doe", Birthdate = new DateOnly(1990, 1, 1), Address = "123 Main St", PhoneNumber = "555-1234", Email = "john.doe@example.com" },
                new Member { Id = Guid.NewGuid(), Name = "Jane", Surname = "Smith", Birthdate = new DateOnly(1985, 5, 5), Address = "456 Oak St", PhoneNumber = "555-5678", Email = "jane.smith@example.com" }
            });
            await context.SaveChangesAsync();

            var controller = new MembersController(context);

            // Act
            var result = await controller.GetMembers(null, null, null, 1, 10, "Surname", true) as OkObjectResult;
            var members = result?.Value as PaginatedList<MemberDto>;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(members, Is.Not.Null);
            Assert.That(members.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetMembersRents_ShouldReturnRents_WithValidParameters()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var memberId = Guid.NewGuid();
            context.Members.Add(new Member
            {
                Id = memberId,
                Name = "John",
                Surname = "Doe",
                Birthdate = new DateOnly(1990, 1, 1),
                Address = "123 Main St",
                PhoneNumber = "555-1234",
                Email = "john.doe@example.com"
            });
            context.Rents.AddRange(new List<Rent>
            {
                new Rent { Id = Guid.NewGuid(), RentDate = new DateOnly(2023, 1, 1), PlannedReturnDate = new DateOnly(2023, 1, 10), MemberId = memberId },
                new Rent { Id = Guid.NewGuid(), RentDate = new DateOnly(2023, 2, 1), PlannedReturnDate = new DateOnly(2023, 2, 10), MemberId = memberId }
            });
            await context.SaveChangesAsync();

            var controller = new MembersController(context);

            // Act
            var result = await controller.GetMembersRents(memberId, null, null, 1, 10, "RentDate", true) as OkObjectResult;
            var rents = result?.Value as PaginatedList<RentDto>;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(rents, Is.Not.Null);
            Assert.That(rents.Count, Is.EqualTo(0));
        }
    }
}
