using Esox.SharpAndRusty.EntityFrameworkCore.Extensions;
using Esox.SharpAndRusty.Extensions;
using Esox.SharpAndRusty.Samples.LibraryAPI.Contexts;
using Esox.SharpAndRusty.Types;
using Microsoft.AspNetCore.Mvc;

namespace Esox.SharpAndRusty.Samples.LibraryAPI.Books
{
    [ApiController]
    [Route("api/books")]
    public class BookController(LibraryContext context) : Controller
    {
        private readonly LibraryContext _context = context;

        [HttpGet]
        public IActionResult Index()
        {
            var books = _context.Books.ToList();
            return Ok(books);
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateBookRequest request)
        {
            var book = new Book
            {
                Title = request.Title,
                Author = request.Author,
                Description = request.Description,
                YearOfPublication = request.YearOfPublication
            };
            _context.Books.Add(book);
            _context.SaveChanges();
            return CreatedAtAction(nameof(Index), new { id = book.Id }, book);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Option<Book> book = await _context.Books.SingleOrNoneAsync(b => b.Id == id);
            return book switch
            {
                Option<Book>.Some some => Ok(some.Value),
                Option<Book>.None => NotFound(),
                _ => throw new InvalidOperationException("Unexpected option type.")
            };
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBookRequest request)
        {
            Option<Book> bookOption = await _context.Books.SingleOrNoneAsync(b => b.Id == id);
            switch (bookOption)
            {
                case Option<Book>.Some some:
                    var bookToUpdate = some.Value;
                    bookToUpdate.Title = request.Title;
                    bookToUpdate.Author = request.Author;
                    bookToUpdate.Description = request.Description;
                    bookToUpdate.YearOfPublication = request.YearOfPublication;
                    _context.Books.Update(bookToUpdate);
                    await _context.SaveChangesAsync();
                    return NoContent();
                case Option<Book>.None:
                    return NotFound();
                default:
                    throw new InvalidOperationException("Unexpected option type.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Option<Book> bookToBeDeleted = await _context.Books.SingleOrNoneAsync(b => b.Id == id);
        
            switch (bookToBeDeleted)
            {
                case Option<Book>.Some some:
                    _context.Books.Remove(some.Value);
                    await _context.SaveChangesAsync();
                    return NoContent();
                case Option<Book>.None:
                    return NotFound();
                default:
                    throw new InvalidOperationException("Unexpected option type.");
            }
        }
    }
}

