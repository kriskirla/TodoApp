using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TodoApp.Enums;

namespace TodoApp.Models;

[Table("todo_items")]
public class TodoItem
{
    [Key]
    [Column("id")]
    public Guid? Id { get; set; }
    [Column("list_id")]
    public Guid? TodoListId { get; set; }
    [Column("item_name")]
    public string? Name { get; set; }
    [Column("content")]
    public string? Description { get; set; }
    [Column("media_url")]
    public string? MediaUrl { get; set; }
    [Column("media_type")]
    public MediaType? MediaType { get; set; }
    [Column("due_date")]
    public DateTime? DueDate { get; set; }
    [Column("item_status")]
    public StatusType? Status { get; set; }
    [Column("item_priority")]
    public PriorityType? Priority { get; set; }
}
