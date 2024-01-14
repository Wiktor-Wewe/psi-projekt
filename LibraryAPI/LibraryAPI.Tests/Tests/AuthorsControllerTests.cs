using LibraryAPI.Controllers;
using LibraryAPI.Entities;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryAPI.Tests.Tests
{
    [Category("AuthorsControllerTests")]
    public class AuthorsControllerTests
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
        public async Task GetAuthors_ShouldReturnAuthors_WithValidParameters()
        {
            // Arrange
            var context = await GetDatabaseContext();
            context.Authors.AddRange(new List<Author>
            {
                new Author { Id = Guid.NewGuid(), Name = "John", Surname = "Doe" },
                new Author { Id = Guid.NewGuid(), Name = "Jane", Surname = "Doe" }
            });
            await context.SaveChangesAsync();

            var controller = new AuthorsController(context);

            // Act
            var result = await controller.GetAuthors(null, 1, 10, "Name", true) as OkObjectResult;
            var authors = result?.Value as PaginatedList<AuthorDto>;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(authors, Is.Not.Null);
            Assert.That(authors.Count, Is.EqualTo(4));
        }

        [Test]
        public async Task CreateAuthor_ShouldCreateAuthor_WithValidData()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var controller = new AuthorsController(context);
            var authorDto = new AuthorDto { Name = "New", Surname = "Author" };

            // Act
            var result = await controller.CreateAuthor(authorDto) as OkObjectResult;
            var createdAuthor = result?.Value as AuthorDto;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(createdAuthor, Is.Not.Null);
            Assert.That(createdAuthor.Name, Is.EqualTo("New"));
            Assert.That(createdAuthor.Surname, Is.EqualTo("Author"));
        }

        [Test]
        public async Task GetAuthor_ShouldReturnAuthor_WithValidId()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var authorId = Guid.NewGuid();
            context.Authors.Add(new Author { Id = authorId, Name = "John", Surname = "Doe" });
            await context.SaveChangesAsync();

            var controller = new AuthorsController(context);

            // Act
            var result = await controller.GetAuthor(authorId) as OkObjectResult;
            var author = result?.Value as AuthorDto;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(author, Is.Not.Null);
            Assert.That(author.Name, Is.EqualTo("John"));
            Assert.That(author.Surname, Is.EqualTo("Doe"));
        }
    }
}