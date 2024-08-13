using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Entities;

public class MenuItem : BaseEntity
{
    [Required]
    [StringLength(100)]
    public required string ItemName { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Price { get; set; }

    [StringLength(50)]
    public string? Category { get; set; }

    [JsonIgnore]
    public virtual ICollection<OrderItem>? OrderItems { get; set; }
}
