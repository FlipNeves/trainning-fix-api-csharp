using Library.After.Errors;

namespace Library.After.Books;

public sealed class Book
{
    public const int IsbnLength = 13;
    public const int TitleMaxLength = 200;
    public const int AuthorMaxLength = 120;

    private Book()
    {
    }

    public Book(string isbn, string title, string author, int totalCopies)
    {
        Isbn = NormalizeIsbn(isbn);
        Update(title, author, totalCopies);
    }

    public int Id { get; private set; }
    public string Isbn { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public int TotalCopies { get; private set; }
    public int AvailableCopies { get; private set; }

    public bool IsAvailable => AvailableCopies > 0;

    public void Update(string title, string author, int totalCopies)
    {
        var onLoan = TotalCopies - AvailableCopies;
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Title is required.");
        if (string.IsNullOrWhiteSpace(author)) throw new DomainException("Author is required.");
        if (totalCopies < 1) throw new DomainException("A book needs at least one copy.");
        if (totalCopies < onLoan)
            throw new DomainException($"\"{Title}\" has {onLoan} on loan, so the total cannot go below that.");

        Title = title.Trim();
        Author = author.Trim();
        TotalCopies = totalCopies;
        AvailableCopies = totalCopies - onLoan;
    }

    public void Checkout()
    {
        if (!IsAvailable) throw new DomainException($"No copies of \"{Title}\" are available.");
        AvailableCopies--;
    }

    public void Return()
    {
        if (AvailableCopies == TotalCopies) throw new DomainException($"Every copy of \"{Title}\" is already on the shelf.");
        AvailableCopies++;
    }

    public void EnsureCanBeRemoved()
    {
        if (AvailableCopies < TotalCopies)
            throw new DomainException($"\"{Title}\" still has copies on loan and cannot be removed.");
    }

    public static string NormalizeIsbn(string isbn)
    {
        var digits = isbn.Replace("-", string.Empty).Trim();
        if (digits.Length != IsbnLength || !digits.All(char.IsAsciiDigit) || Checksum(digits) % 10 != 0)
            throw new DomainException($"\"{isbn}\" is not a valid ISBN-13.");

        return digits;
    }

    private static int Checksum(string digits)
        => digits.Select((digit, index) => (digit - '0') * (index % 2 == 0 ? 1 : 3)).Sum();
}
