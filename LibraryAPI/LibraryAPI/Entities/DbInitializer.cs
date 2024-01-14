using LibraryAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace LibraryAPI.Entities
{
    public class DbInitializer
    {
        public static void Initialize(AppDbContext context, IPasswordHasher<Employee> passwordHasher)
        {
            context.Database.EnsureCreated();

            if (!context.Authors.Any())
            {
                var authors = new Author[]
                {
                    new Author
                    {
                        Name = "Stephen",
                        Surname = "King"
                    },
                    new Author
                    {
                        Name = "Joe",
                        Surname = "Hill"
                    }
                };

                context.Authors.AddRange(authors);
                context.SaveChanges();
            }

            if (!context.Genres.Any())
            {
                var genres = new Genre[]
                {
                    new Genre
                    {
                        Name = "Horror",
                        Description = "Horror is a genre of fiction that is intended to disturb, frighten or scare."
                    },
                    new Genre
                    {
                        Name = "Thriller",
                        Description = "Thriller is a genre of fiction with numerous, often overlapping, subgenres, including crime, horror, and detective fiction."
                    }
                };

                context.Genres.AddRange(genres);
                context.SaveChanges();
            }

            if (!context.PublishingHouses.Any())
            {
                var publishingHouses = new PublishingHouse[]
                {
                    new PublishingHouse
                    {
                        Name = "Hodder & stoughton",
                        FoundationYear = 1868,
                        Website = "https://www.hodder.co.uk/"
                    },
                    new PublishingHouse
                    {
                        Name = "William Morrow Paperbacks",
                        FoundationYear = 1926,
                        Address = "10 East 53rd Street New York NY 10022 USA"
                    }
                };

                context.PublishingHouses.AddRange(publishingHouses);
                context.SaveChanges();
            }

            if (!context.Employees.Any())
            {
                var employee = new Employee
                {
                    Name = "admin",
                    Surname = "admin",
                    JobPosition = "admin",
                };

                employee.PasswordHash = passwordHasher.HashPassword(employee, "admin");
                context.Employees.Add(employee);
                context.SaveChanges();
            }

            if (!context.Members.Any())
            {
                var members = new Member[]
                {
                    new Member
                    {
                        Name = "Joe",
                        Surname = "Smith",
                        Birthdate = new DateTime(1998, 3, 2),
                        PhoneNumber = "983-231-231",
                        Rents = new List<Rent>()
                    },
                    new Member
                    {
                        Name = "John",
                        Surname = "Johnson",
                        Birthdate = new DateTime(1999, 6, 20),
                        Email = "John.Johanson@gmail.com",
                        Rents = new List<Rent>(),
                        PhoneNumber = "987-241-231"
                    }
                };

                context.Members.AddRange(members);
                context.SaveChanges();
            }

            if (!context.Books.Any())
            {
                var books = new Book[]
                {
                    new Book
                    {
                        Title = "It",
                        Description = "It is a 1986 horror novel by American author Stephen King. It was his 22nd book and his 17th novel written under his own name.",
                        RelaseDate = new DateTime(1986, 8, 15),
                        ISBN = "0-670-81302-8",
                        Genres = context.Genres.Take(1).ToList(),
                        Authors = context.Authors.Take(1).ToList(),
                        PublishingHouse = context.PublishingHouses.FirstOrDefault(x => x.Name != " ")
                    },
                    new Book{
                        Title = "nos4a2",
                        ISBN = "9788381253673",
                        Genres = context.Genres.Take(1).ToList(),
                        Authors = context.Authors.Take(1).ToList(),
                        PublishingHouse = context.PublishingHouses.FirstOrDefault(p => p.Name != " ")
                    }
                };

                context.Books.AddRange(books);
                context.SaveChanges();
            }

            if (!context.Rents.Any())
            {
                var rents = new Rent[]
                {
                    new Rent
                    {
                        RentDate = new DateTime(2023, 8, 23),
                        PlannedReturnDate = new DateTime(2023, 9, 23),
                        ReturnDate = new DateTime(2023, 9, 2),
                        Member = context.Members.FirstOrDefault(m => m.Name != " "),
                        Books = context.Books.Take(1).ToList(),
                        Employee = context.Employees.FirstOrDefault(e => e.Name != " ")
                    },
                    new Rent
                    {
                        RentDate = new DateTime(2023, 9, 5),
                        PlannedReturnDate = new DateTime(2023, 9, 5),
                        Member = context.Members.FirstOrDefault(m => m.Name != " "),
                        Books = context.Books.Take(1).ToList(),
                        Employee = context.Employees.FirstOrDefault(e => e.Name != " ")
                    },
                    new Rent
                    {
                        RentDate = new DateTime(2023, 11, 12),
                        PlannedReturnDate = new DateTime(2023, 12, 12),
                        Member = context.Members.FirstOrDefault(m => m.Name != " "),
                        Books = context.Books.ToList(),
                        Employee = context.Employees.FirstOrDefault(e => e.Name != " ")
                    }
                };

                context.Rents.AddRange(rents);
                context.SaveChanges();
            }
        }
    }
}
