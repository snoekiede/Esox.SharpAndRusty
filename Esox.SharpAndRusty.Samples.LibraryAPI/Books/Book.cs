using Esox.SharpAndRusty.Samples.LibraryAPI.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace Esox.SharpAndRusty.Samples.LibraryAPI.Books
{
    public record Book:BaseModel
    {
        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;
        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string Author { get; set; } = string.Empty;
        [Required]
        [StringLength(255, MinimumLength = 1)] 
        public string Description { get; set; } = string.Empty;
        public int YearOfPublication { get; set; }
    }
}
