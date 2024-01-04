using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<PublishingHouse> PublishingHouses { get; set; }
        public DbSet<Rent> Rents { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rent>()
                .HasMany(r => r.Books)
                .WithMany(r => r.Rent);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Authors)
                .WithMany(b => b.Books);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Genres)
                .WithMany(b => b.Books);

            modelBuilder.Entity<Rent>()
                .HasOne(r => r.Member)
                .WithMany(r => r.Rents)
                .HasForeignKey(r => r.MemberId)
                .IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }
}
