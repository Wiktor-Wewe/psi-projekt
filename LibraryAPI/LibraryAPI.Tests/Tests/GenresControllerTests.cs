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
    [Category("GenresControllerTests")]
    public class GenresControllerTests
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
        public async Task GetGenres_ShouldReturnGenres_WithValidParameters()
        {
            // Arrange
            var context = await GetDatabaseContext();
            context.Genres.AddRange(new List<Genre>
            {
                new Genre { Id = Guid.NewGuid(), Name = "Fantasy", Description = "Genre about magical worlds" },
                new Genre { Id = Guid.NewGuid(), Name = "Science Fiction", Description = "Genre about futuristic science and technology" }
            });
            await context.SaveChangesAsync();

            var controller = new GenresController(context);

            // Act
            var result = await controller.GetGenres(null, 1, 10, "Name", true) as OkObjectResult;
            var genres = result?.Value as PaginatedList<GenreDto>;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(genres, Is.Not.Null);
            Assert.That(genres.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task CreateGenre_ShouldCreateGenre_WithValidData()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var controller = new GenresController(context);
            var genreDto = new GenreDto { Name = "New Genre", Description = "Description of the new genre" };

            // Act
            var result = await controller.CreateGenre(genreDto) as OkObjectResult;
            var createdGenre = result?.Value as GenreDto;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(createdGenre, Is.Not.Null);
            Assert.That(createdGenre.Name, Is.EqualTo("New Genre"));
            Assert.That(createdGenre.Description, Is.EqualTo("Description of the new genre"));
        }
    }
}