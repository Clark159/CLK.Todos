using System.ComponentModel.DataAnnotations;

namespace CLK.Todos;

public class User
{
    // Properties
    public Guid UserId { get; set; } = Guid.CreateVersion7();

    [Required(ErrorMessage = "不可以為空白")]
    [StringLength(50, ErrorMessage = "長度不可超過 50 字")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "不可以為空白")]
    [EmailAddress(ErrorMessage = "格式不正確")]
    [StringLength(100, ErrorMessage = "長度不可超過 100 字")]
    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;


    // Methods
    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}
