using System;
using System.Linq;
using Library.Before.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Before.Data
{
    public class LibraryContext : DbContext
    {
        public DbSet<Book> Books { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("library");
        }

        public static void Seed()
        {
            var context = new LibraryContext();
            if (context.Books.Any())
                return;

            context.Books.Add(new Book { Id = 1, Isbn = "9780321125217", Title = "Domain-Driven Design", Author = "Eric Evans", TotalCopies = 2, AvailableCopies = 2, Status = "A", CreatedAt = DateTime.Now });
            context.Books.Add(new Book { Id = 2, Isbn = "9780134757599", Title = "Refactoring", Author = "Martin Fowler", TotalCopies = 3, AvailableCopies = 3, Status = "A", CreatedAt = DateTime.Now });
            context.Books.Add(new Book { Id = 3, Isbn = "9780134494166", Title = "Clean Architecture", Author = "Robert C. Martin", TotalCopies = 1, AvailableCopies = 1, Status = "A", CreatedAt = DateTime.Now });
            context.SaveChanges();
        }
    }
}
