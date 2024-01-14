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
    [Category("PublishingHousesControllerTests")]
    public class PublishingHousesControllerTests
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
        public async Task GetPublishingHouses_ShouldReturnPublishingHouses_WithValidParameters()
        {
            // Arrange
            var context = await GetDatabaseContext();
            context.PublishingHouses.AddRange(new List<PublishingHouse>
            {
                new PublishingHouse { Id = Guid.NewGuid(), Name = "Publisher A", FoundationYear = 2000, Address = "123 Main St", Website = "www.publisherA.com" },
                new PublishingHouse { Id = Guid.NewGuid(), Name = "Publisher B", FoundationYear = 1995, Address = "456 Oak St", Website = "www.publisherB.com" }
            });
            await context.SaveChangesAsync();

            var controller = new PublishingHousesController(context);

            // Act
            var result = await controller.GetPublishingHouses(null, null, null, 1, 10, "Name", true) as OkObjectResult;
            var publishingHouses = result?.Value as PaginatedList<PublishingHouseDto>;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(publishingHouses, Is.Not.Null);
            Assert.That(publishingHouses.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task CreatePublishingHouse_ShouldCreatePublishingHouse_WithValidData()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var controller = new PublishingHousesController(context);
            var publishingHouseDto = new PublishingHouseDto
            {
                Name = "New Publisher",
                FoundationYear = 2005,
                Address = "789 Pine St",
                Website = "www.newpublisher.com"
            };

            // Act
            var result = await controller.CreatePublishingHouse(publishingHouseDto) as OkObjectResult;
            var createdPublishingHouse = result?.Value as PublishingHouseDto;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(createdPublishingHouse, Is.Not.Null);
            Assert.That(createdPublishingHouse.Name, Is.EqualTo("New Publisher"));
            Assert.That(createdPublishingHouse.FoundationYear, Is.EqualTo(2005));
            Assert.That(createdPublishingHouse.Address, Is.EqualTo("789 Pine St"));
            Assert.That(createdPublishingHouse.Website, Is.EqualTo("www.newpublisher.com"));
        }
    }
}
