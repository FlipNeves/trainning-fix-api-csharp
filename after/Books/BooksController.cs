using Library.After.Data;
using Library.After.OpenLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.After.Books;

[ApiController]
[Route("books")]
public sealed class BooksController : ControllerBase
{
    private readonly LibraryDbContext _db;

    public BooksController(LibraryDbContext db) => _db = db;

    [HttpGet]
    public async Task<IReadOnlyList<BookResponse>> List([FromQuery] string? title, CancellationToken ct)
    {
        var books = _db.Books.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(title))
            books = books.Where(b => EF.Functions.Like(b.Title, $"%{title}%"));

        var found = await books.OrderBy(b => b.Title).ToListAsync(ct);
        return found.Select(BookResponse.From).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookResponse>> Get(int id, CancellationToken ct)
    {
        var book = await _db.Books.FindAsync([id], ct);
        if (book is null) return NotFound();

        return BookResponse.From(book);
    }

    [HttpPost]
    public async Task<ActionResult<BookResponse>> Create(CreateBookRequest request, CancellationToken ct)
    {
        var book = new Book(request.Isbn, request.Title, request.Author, request.TotalCopies);
        _db.Books.Add(book);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Get), new { id = book.Id }, BookResponse.From(book));
    }

    [HttpPut("{id:int}")]
    public Task<ActionResult<BookResponse>> Update(int id, UpdateBookRequest request, CancellationToken ct)
        => ChangeAsync(id, book => book.Update(request.Title, request.Author, request.TotalCopies), ct);

    [HttpPost("{id:int}/checkout")]
    public Task<ActionResult<BookResponse>> Checkout(int id, CancellationToken ct)
        => ChangeAsync(id, book => book.Checkout(), ct);

    [HttpPost("{id:int}/return")]
    public Task<ActionResult<BookResponse>> Return(int id, CancellationToken ct)
        => ChangeAsync(id, book => book.Return(), ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var book = await _db.Books.FindAsync([id], ct);
        if (book is null) return NotFound();

        book.EnsureCanBeRemoved();
        _db.Books.Remove(book);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpGet("isbn/{isbn}")]
    public async Task<ActionResult<OpenLibraryEdition>> Lookup(string isbn, [FromServices] OpenLibraryClient openLibrary, CancellationToken ct)
    {
        var edition = await openLibrary.FindByIsbnAsync(Book.NormalizeIsbn(isbn), ct);
        if (edition is null) return NotFound();

        return edition;
    }

    private async Task<ActionResult<BookResponse>> ChangeAsync(int id, Action<Book> change, CancellationToken ct)
    {
        var book = await _db.Books.FindAsync([id], ct);
        if (book is null) return NotFound();

        change(book);
        await _db.SaveChangesAsync(ct);

        return BookResponse.From(book);
    }
}
