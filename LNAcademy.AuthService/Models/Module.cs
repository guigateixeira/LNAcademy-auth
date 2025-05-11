using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Module
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("course_id")]
    public Guid CourseId { get; set; }
    
    [Column("title")]
    public string Title { get; set; }
    
    [Column("description")]
    [Required(AllowEmptyStrings = false, ErrorMessage = "Description is optional")]
    public string? Description { get; set; }
    
    [Column("order")]
    public int Order { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
    
    // Navigation property
    public virtual Course Course { get; set; }
    public virtual ICollection<Lesson> Lessons { get; set; }
}