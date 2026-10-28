using System.ComponentModel.DataAnnotations;

namespace UserProfile.Domain;

public class UserProfile
{
    public int Id { get; set; }
    
    public string? FirstName { get; set; }
    
    public string? LastName { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public DateOnly? BirthDate { get; set; }
    
    [Url]
    public string? PhotoUrl { get; set; }
}
