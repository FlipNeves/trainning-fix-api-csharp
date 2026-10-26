namespace Library.After.Books;

public sealed record BookResponse(
    int Id,
    string Isbn,
    string Title,
    string Author,
    int TotalCopies,
    int AvailableCopies,
    bool IsAvailable)
{
    public static BookResponse From(Book book)
        => new(book.Id, book.Isbn, book.Title, book.Author, book.TotalCopies, book.AvailableCopies, book.IsAvailable);
}
