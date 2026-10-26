using System.ComponentModel.DataAnnotations;

namespace Library.After.Books;

public sealed record UpdateBookRequest(
    [Required, StringLength(Book.TitleMaxLength)] string Title,
    [Required, StringLength(Book.AuthorMaxLength)] string Author,
    [Range(1, int.MaxValue, ErrorMessage = "TotalCopies must be at least 1.")] int TotalCopies);
