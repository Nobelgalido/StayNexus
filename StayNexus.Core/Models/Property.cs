namespace StayNexus.Core.Models;

public class Property
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Per-property branding
    public string? PrimaryColor { get; set; }
    public string? LogoUrl { get; set; }
    public string? HeroImageUrl { get; set; }
    public string? Tagline { get; set; }

    // Navigation properties
    public ApplicationUser Owner { get; set; } = null!;
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}