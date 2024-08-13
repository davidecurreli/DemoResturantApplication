using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Entities;

public class Customer : BaseEntity
{
    [Required]
    [StringLength(50)]
    public required string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public required string LastName { get; set; }

    [Required]
    [StringLength(100)]
    [EmailAddress]
    public required string Email { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [JsonIgnore]
    public virtual ICollection<Order>? Orders { get; set; }
}

