using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Book : Product
{
    public Book()
    {
        Type = ProductType.Book;
    }
    
    [Column("author")]
    public string Author { get; set; }
    
    [Column("language")]
    [Required(AllowEmptyStrings = false, ErrorMessage = "Language is optional")]
    public string Language { get; set; }
    
    [Column("format")]
    [Required(AllowEmptyStrings = false, ErrorMessage = "Format is optional")]
    public string? Format { get; set; }
    
    [Column("download_url")]
    public string DownloadUrl { get; set; }
    
    [Column("preview_url")]
    public string PreviewUrl { get; set; }
}