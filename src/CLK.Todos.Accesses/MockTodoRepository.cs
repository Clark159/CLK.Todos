using CLK.Todos;

namespace CLK.Todos.Accesses
{
    public class MockTodoRepository : ITodoRepository
    {
        // Fields
        private readonly List<Todo> _todos = new();

        private readonly object _lock = new();

        private int _nextId = 1;


        // Methods
        public Todo Add(Todo todo)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(todo);

            #endregion

            lock (_lock)
            {
                // Add
                todo.Id = _nextId++;
                _todos.Add(todo);

                // Return
                return todo;
            }
        }

        public bool Update(Todo todo)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(todo);

            #endregion

            lock (_lock)
            {
                // FindById
                var existing = _todos.FirstOrDefault(t => t.Id == todo.Id);
                if (existing is null)
                {
                    return false;
                }

                // Update
                existing.Title = todo.Title;
                existing.IsDone = todo.IsDone;

                // Return
                return true;
            }
        }

        public bool Remove(int id)
        {
            lock (_lock)
            {
                // FindById
                var existing = _todos.FirstOrDefault(t => t.Id == id);
                if (existing is null)
                {
                    return false;
                }

                // Remove
                _todos.Remove(existing);

                // Return
                return true;
            }
        }

        public Todo FindById(int id)
        {
            lock (_lock)
            {
                // Return
                return _todos.FirstOrDefault(t => t.Id == id);
            }
        }

        public IReadOnlyList<Todo> FindAll()
        {
            lock (_lock)
            {
                // Return
                return _todos
                    .OrderBy(t => t.IsDone)
                    .ThenByDescending(t => t.CreatedAt)
                    .ToList();
            }
        }
    }
}
