using System.ComponentModel.DataAnnotations;

namespace CLK.Todos;

public class Todo
{
    // Properties
    public Guid TodoId { get; set; } = Guid.CreateVersion7();

    [Required(ErrorMessage = "標題不可為空")]
    [StringLength(100, ErrorMessage = "標題長度不可超過 100 字")]
    public string Title { get; set; } = string.Empty;

    public bool IsDone { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;


    // Methods
    public void ToggleDone()
    {
        IsDone = !IsDone;
    }
}
