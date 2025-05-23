using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TodoApp.Enums;

namespace TodoApp.Models;

[Table("todo_list_shares")]
public class TodoListShare
{
    [Key]
    [Column("id")]
    public Guid? Id { get; set; }
    [Column("list_id")]
    public Guid? TodoListId { get; set; }
    [Column("user_id")]
    public Guid? SharedWithUserId { get; set; }
    [Column("permission")]
    public PermissionType? Permission { get; set; }
}
