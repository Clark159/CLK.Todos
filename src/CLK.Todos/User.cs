using System.ComponentModel.DataAnnotations;

namespace CLK.Todos;

public class User
{
    // Properties
    public Guid UserId { get; set; } = Guid.CreateVersion7();

    [Required(ErrorMessage = "使用者名稱不可為空")]
    [StringLength(50, ErrorMessage = "使用者名稱長度不可超過 50 字")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email 不可為空")]
    [EmailAddress(ErrorMessage = "Email 格式不正確")]
    [StringLength(100, ErrorMessage = "Email 長度不可超過 100 字")]
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
