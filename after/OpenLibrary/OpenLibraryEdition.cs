namespace Library.After.OpenLibrary;

public sealed record OpenLibraryEdition(string? Title, string? PublishDate, int? NumberOfPages, IReadOnlyList<string>? Publishers);
