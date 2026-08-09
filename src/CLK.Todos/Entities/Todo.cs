using System.ComponentModel.DataAnnotations;

namespace CLK.Todos.Entities;

public class Todo
{
    public int Id { get; set; }

    [Required(ErrorMessage = "請輸入待辦事項標題")]
    [StringLength(100, ErrorMessage = "標題長度不可超過 100 字")]
    public string Title { get; set; } = string.Empty;

    public bool IsDone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
