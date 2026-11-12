namespace Esox.SharpAndRusty.Samples.LibraryAPI.Models.Base
{
    public record BaseModel
    {
        public int Id { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
    }
}
