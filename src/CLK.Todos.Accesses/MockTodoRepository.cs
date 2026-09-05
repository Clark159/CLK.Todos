using CLK.Todos;

namespace CLK.Todos.Accesses
{
    public class MockTodoRepository : ITodoRepository
    {
        // Fields
        private readonly List<Todo> _todos = new();

        private readonly object _lock = new();


        // Methods
        public Todo Add(Todo todo)
        {
            // Contracts
            ArgumentNullException.ThrowIfNull(todo);

            // Lock
            lock (_lock)
            {
                // Execute
                todo.Id = Guid.CreateVersion7();
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
                var existing = _todos.FirstOrDefault(t => t.Id == todo.Id);
                if (existing is null) return false;

                // Execute
                existing.Title = todo.Title;
                existing.IsDone = todo.IsDone;

                // Result
                return true;
            }
        }

        public bool Remove(Guid id)
        {
            // Lock
            lock (_lock)
            {
                // Search
                var existing = _todos.FirstOrDefault(t => t.Id == id);
                if (existing is null) return false;

                // Execute
                _todos.Remove(existing);

                // Result
                return true;
            }
        }

        public Todo FindById(Guid id)
        {
            // Lock
            lock (_lock)
            {
                // Result
                return _todos.FirstOrDefault(t => t.Id == id);
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
                    .ThenByDescending(t => t.CreatedAt)
                    .ToList();
            }
        }
    }
}
