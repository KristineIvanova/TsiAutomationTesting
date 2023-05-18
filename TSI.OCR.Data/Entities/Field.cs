using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TSI.OCR.Data.Entities;

public class Field {
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string Value { get; set; } = default!;
    
    [Required]
    public int X { get; set; }
    
    [Required]
    public int Y { get; set; }
    
    [Required]
    public int Width { get; set; }
    
    [Required]
    public int Height { get; set; }
    
    [Required]
    public int DocumentId { get; set; }
    
    public Document Document { get; set; }
    
    public int Page { get; set; }

    public byte[]? Image { get; set; }
}