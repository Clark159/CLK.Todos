using CLK.Todos;

namespace CLK.Todos.Accesses;

public class MockTodoRepository : ITodoRepository
{
    // Fields
    private readonly object _lock = new();

    private readonly List<Todo> _todos = new();


    // Methods
    public void Add(Todo todo)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(todo);

        // Lock
        lock (_lock)
        {
            // Execute
            todo.CreateTime = DateTime.UtcNow;
            todo.UpdateTime = DateTime.UtcNow;
            _todos.Add(todo);
        }
    }

    public void Update(Todo todo)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(todo);

        // Lock
        lock (_lock)
        {
            // Search
            var entity = _todos.FirstOrDefault(t => t.TodoId == todo.TodoId);
            if (entity is null) throw new KeyNotFoundException($"Todo not found: {todo.TodoId}");

            // Execute
            entity.Title = todo.Title;
            entity.IsDone = todo.IsDone;
            entity.UpdateTime = DateTime.UtcNow;
        }
    }

    public void Remove(Guid todoId)
    {
        // Lock
        lock (_lock)
        {
            // Search
            var entity = _todos.FirstOrDefault(t => t.TodoId == todoId);
            if (entity is null) throw new KeyNotFoundException($"Todo not found: {todoId}");

            // Execute
            _todos.Remove(entity);
        }
    }

    public Todo? FindById(Guid todoId)
    {
        // Lock
        lock (_lock)
        {
            // Return
            return _todos.FirstOrDefault(t => t.TodoId == todoId);
        }
    }

    public IReadOnlyList<Todo> FindAll()
    {
        // Lock
        lock (_lock)
        {
            // Return
            return _todos
                .OrderBy(t => t.IsDone)
                .ThenByDescending(t => t.CreateTime)
                .ToList();
        }
    }
}
