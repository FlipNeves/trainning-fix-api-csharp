using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Library.Before.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Library.Before.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BookController : BaseController
    {
        private static Dictionary<string, string> _isbnCache = new Dictionary<string, string>();
        private readonly ILogger<BookController> _logger;

        public BookController(ILogger<BookController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ResultModel GetBooks(string title = null)
        {
            try
            {
                var query = Db.Books.AsQueryable();
                if (!string.IsNullOrEmpty(title))
                    query = query.Where(b => b.Title.Contains(title));

                var books = query.ToListAsync().Result;
                return new ResultModel { success = true, code = 200, data = books };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error listing books: {ex.Message} - {ex.StackTrace}");
                return new ResultModel { success = false, code = 500, message = ex.Message };
            }
        }

        [HttpGet]
        public ResultModel GetBook(int id)
        {
            var book = Db.Books.FirstOrDefault(b => b.Id == id);
            if (book == null)
                return new ResultModel { success = false, code = 404, message = "Book not found" };

            return new ResultModel { success = true, code = 200, data = book };
        }

        [HttpPost]
        public ResultModel SaveBook([FromBody] Book book)
        {
            if (book.Title == null || book.Title == "")
                return new ResultModel { success = false, code = 400, message = "Title is required" };

            if (book.Isbn == null || (book.Isbn.Length != 10 && book.Isbn.Length != 13))
                return new ResultModel { success = false, code = 400, message = "Invalid ISBN" };

            book.Id = Db.Books.Count() == 0 ? 1 : Db.Books.Max(b => b.Id) + 1;
            book.Status = "A";
            book.CreatedAt = DateTime.Now;

            Db.Books.Add(book);
            Db.SaveChanges();

            return new ResultModel { success = true, code = 200, data = book };
        }

        [HttpPut]
        public ResultModel UpdateBook([FromBody] Book book)
        {
            try
            {
                var existing = Db.Books.FirstOrDefault(b => b.Id == book.Id);
                if (existing == null)
                    throw new Exception("Book not found");

                existing.Title = book.Title;
                existing.Author = book.Author;
                existing.Isbn = book.Isbn;
                existing.TotalCopies = book.TotalCopies;
                existing.AvailableCopies = book.AvailableCopies;

                Db.SaveChanges();
                return new ResultModel { success = true, code = 200, data = existing };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating book {book.Id}: {ex.Message}");
                throw ex;
            }
        }

        [HttpDelete]
        public bool DeleteBook(int id)
        {
            try
            {
                var book = Db.Books.First(b => b.Id == id);
                Db.Books.Remove(book);
                Db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        [HttpPost]
        public ResultModel Checkout(int id)
        {
            var book = Db.Books.FirstOrDefault(b => b.Id == id);
            if (book == null)
                return new ResultModel { success = false, code = 404, message = "Book not found" };

            if (book.Status != "A" || book.AvailableCopies <= 0)
                return new ResultModel { success = false, code = 400, message = "Book not available" };

            book.AvailableCopies = book.AvailableCopies - 1;
            if (book.AvailableCopies == 0)
                book.Status = "U";

            Db.SaveChanges();
            return new ResultModel { success = true, code = 200, data = book };
        }

        [HttpPost]
        public ResultModel Return(int id)
        {
            var book = Db.Books.FirstOrDefault(b => b.Id == id);
            if (book == null)
                return new ResultModel { success = false, code = 404, message = "Book not found" };

            book.AvailableCopies = book.AvailableCopies + 1;
            Db.SaveChanges();
            return new ResultModel { success = true, code = 200, data = book };
        }

        [HttpGet]
        public string LookupIsbn(string isbn)
        {
            if (_isbnCache.ContainsKey(isbn))
                return _isbnCache[isbn];

            var client = new HttpClient();
            var json = client.GetStringAsync(Startup.Configuration["OpenLibraryUrl"] + "isbn/" + isbn + ".json").Result;
            _isbnCache[isbn] = json;
            return json;
        }
    }
}
