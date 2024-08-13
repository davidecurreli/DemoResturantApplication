using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Entities;

public class Order : BaseEntity
{
    [Required]
    public required int CustomerID { get; set; }

    [Required]
    public required DateTime OrderDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public required decimal TotalAmount { get; set; }

    [Required]
    [StringLength(20)]
    public required string Status { get; set; }

    [ForeignKey("CustomerID")]
    public Customer? Customer { get; set; }

    [JsonIgnore]
    public virtual ICollection<OrderItem>? OrderItems { get; set; }
}

