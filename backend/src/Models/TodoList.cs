using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApp.Models;

[Table("todo_lists")]
public class TodoList
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    [Column("title")]
    public string? Title { get; set; }
    [Required]
    [Column("user_id")]
    public Guid OwnerId { get; set; }
    public List<TodoItem> Items { get; set; } = [];
    public List<TodoListShare> SharedWith { get; set; } = [];
}
