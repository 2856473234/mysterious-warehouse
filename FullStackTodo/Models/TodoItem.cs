using System.ComponentModel.DataAnnotations;

namespace FullStackTodo.Models;

public class TodoItem
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "标题不能为空")]
    [MaxLength(200)]
    public string Title { get; set; } = "";

    public bool IsDone { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
