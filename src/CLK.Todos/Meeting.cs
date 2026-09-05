using System.ComponentModel.DataAnnotations;

namespace CLK.Todos;

public class Meeting
{
    // Properties
    public Guid MeetingId { get; set; } = Guid.CreateVersion7();

    [Required(ErrorMessage = "不可以為空白")]
    [StringLength(100, ErrorMessage = "長度不可超過 100 字")]
    public string Title { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    [StringLength(100, ErrorMessage = "長度不可超過 100 字")]
    public string? Location { get; set; } = string.Empty;

    public bool IsCancelled { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;


    // Methods
    public void ToggleCancelled()
    {
        IsCancelled = !IsCancelled;
    }
}
