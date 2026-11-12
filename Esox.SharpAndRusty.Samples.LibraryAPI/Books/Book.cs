using Esox.SharpAndRusty.Samples.LibraryAPI.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace Esox.SharpAndRusty.Samples.LibraryAPI.Books
{
    public record Book:BaseModel
    {
        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string Title { get; init; } = string.Empty;
        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string Author { get; init; } = string.Empty;
        [Required]
        [StringLength(255, MinimumLength = 1)] 
        public string Description { get; init; } = string.Empty;
        public int YearOfPublication { get; init; }
    }
}
