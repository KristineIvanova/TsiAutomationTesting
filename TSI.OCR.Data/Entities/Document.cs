using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TSI.OCR.Data.Entities; 

public class Document {
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [StringLength(1024)]
    [Required]
    public string Name { get; set; } = default!;

    public ICollection<Field> Fields { get; set; } = default!;
}