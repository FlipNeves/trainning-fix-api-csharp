using Library.After.Books;
using Microsoft.EntityFrameworkCore;

namespace Library.After.Data;

public sealed class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();

    public static async Task SeedAsync(DbContext context, bool storeCreated, CancellationToken ct)
    {
        var books = context.Set<Book>();
        if (await books.AnyAsync(ct)) return;

        books.AddRange(
            new Book("9780321125217", "Domain-Driven Design", "Eric Evans", 2),
            new Book("9780134757599", "Refactoring", "Martin Fowler", 3),
            new Book("9780134494166", "Clean Architecture", "Robert C. Martin", 1));

        await context.SaveChangesAsync(ct);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Book>(book =>
        {
            book.Property(b => b.Isbn).HasMaxLength(Book.IsbnLength);
            book.Property(b => b.Title).HasMaxLength(Book.TitleMaxLength);
            book.Property(b => b.Author).HasMaxLength(Book.AuthorMaxLength);
        });
}
