namespace CLK.Todos.Accesses;
using CLK.Todos;

public class MockTodoRepository : ITodoRepository
{
    // Fields
    private readonly object _lock = new();

    private readonly List<Todo> _todos = new();


    // Methods
    public Todo Add(Todo todo)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(todo);

        // Lock
        lock (_lock)
        {
            // Execute
            todo.TodoId = Guid.CreateVersion7();
            _todos.Add(todo);

            // Result
            return todo;
        }
    }

    public bool Update(Todo todo)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(todo);

        // Lock
        lock (_lock)
        {
            // Search
            var existing = _todos.FirstOrDefault(t => t.TodoId == todo.TodoId);
            if (existing is null) return false;

            // Execute
            existing.Title = todo.Title;
            existing.IsDone = todo.IsDone;
            existing.UpdateTime = DateTime.UtcNow;

            // Result
            return true;
        }
    }

    public bool Remove(Guid todoId)
    {
        // Lock
        lock (_lock)
        {
            // Search
            var existing = _todos.FirstOrDefault(t => t.TodoId == todoId);
            if (existing is null) return false;

            // Execute
            _todos.Remove(existing);

            // Result
            return true;
        }
    }

    public Todo FindById(Guid todoId)
    {
        // Lock
        lock (_lock)
        {
            // Result
            return _todos.FirstOrDefault(t => t.TodoId == todoId);
        }
    }

    public IReadOnlyList<Todo> FindAll()
    {
        // Lock
        lock (_lock)
        {
            // Result
            return _todos
                .OrderBy(t => t.IsDone)
                .ThenByDescending(t => t.CreateTime)
                .ToList();
        }
    }
}
