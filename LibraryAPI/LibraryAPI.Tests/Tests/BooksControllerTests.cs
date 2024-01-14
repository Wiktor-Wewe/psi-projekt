using LibraryAPI.Controllers;
using LibraryAPI.Entities;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Tests.Tests
{
    [Category("BooksControllerTests")]
    public class BooksControllerTests
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
        public async Task GetBooks_ShouldReturnBooks_WithValidParameters()
        {
            // Arrange
            var context = await GetDatabaseContext();
            context.Books.AddRange(new List<Book>
            {
                new Book { Id = Guid.NewGuid(), Title = "Book 1", ISBN = "1234567890" },
                new Book { Id = Guid.NewGuid(), Title = "Book 2", ISBN = "0987654321" }
            });
            await context.SaveChangesAsync();

            var controller = new BooksController(context);

            // Act
            var result = await controller.GetBooks(null, null, null, 1, 10, "Title", true) as OkObjectResult;
            var books = result?.Value as PaginatedList<BookDto>;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(books, Is.Not.Null);
            Assert.That(books.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task DeleteBook_ShouldDeleteBook_WithValidId()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var bookId = Guid.NewGuid();
            context.Books.Add(new Book { Id = bookId, Title = "Book to delete", ISBN = "999888777" });
            await context.SaveChangesAsync();

            var controller = new BooksController(context);

            // Act
            var result = await controller.DeleteBook(bookId) as OkResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));

            var deletedBook = await context.Books.FindAsync(bookId);
            Assert.That(deletedBook, Is.Null);
        }
    }
}
