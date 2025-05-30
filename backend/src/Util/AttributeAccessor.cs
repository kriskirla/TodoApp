using System.Linq.Expressions;
using TodoApp.Models;

namespace TodoApp.Util;

public class AttributeAccessors
{
    public Func<TodoItem, object, bool> FilterPredicate { get; set; } = default!;
    public Expression<Func<TodoItem, object?>> SortSelector { get; set; } = default!;
}
