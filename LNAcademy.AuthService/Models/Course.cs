using System.ComponentModel.DataAnnotations.Schema;

public class Course : Product
{
    public Course()
    {
        Type = ProductType.Course;
    }
    
    [Column("level")]
    public string Level { get; set; } // Beginner, Intermediate, Advanced
    
    // Navigation properties
    public virtual ICollection<Module> Modules { get; set; }
}
