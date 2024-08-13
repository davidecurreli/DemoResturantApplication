using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class OrderItem : BaseEntity
{
    [Required]
    public int OrderID { get; set; }

    [Required]
    public int MenuItemID { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal UnitPrice { get; set; }

    [ForeignKey("OrderID")]
    public Order? Order { get; set; }

    [ForeignKey("MenuItemID")]
    public MenuItem? MenuItem { get; set; }
}
