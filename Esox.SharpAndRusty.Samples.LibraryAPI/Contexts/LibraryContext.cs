using Esox.SharpAndRusty.Samples.LibraryAPI.Books;
using Microsoft.EntityFrameworkCore;

namespace Esox.SharpAndRusty.Samples.LibraryAPI.Contexts
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
    }
}
