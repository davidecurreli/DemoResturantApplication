using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Domain;

public class BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required(ErrorMessage = "Il campo 'CreatedOn' è obbligatorio.")]
    public DateTime CreatedOn { get; set; }
    [Required(ErrorMessage = "Il campo 'UpdatedOn' è obbligatorio.")]
    public DateTime UpdatedOn { get; set; }
}
