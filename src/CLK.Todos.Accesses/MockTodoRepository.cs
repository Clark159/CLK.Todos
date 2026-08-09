// Imports
using CLK.Todos;


namespace CLK.Todos.Accesses
{
    /// <summary>
    /// Mock 實作：用記憶體內 List 存資料，重啟程式就會清空。
    /// 用 lock 保護，避免多個請求同時新增/修改造成資料錯亂。
    /// 之後若要換成真正的資料庫，只要在這個專案（或未來的實作專案）另外新增
    /// ITodoRepository 的實作類別即可，Domain 跟呼叫端都不用改。
    /// </summary>
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
                todo.Id = _nextId++;
                _todos.Add(todo);
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
                var existing = _todos.FirstOrDefault(t => t.Id == todo.Id);
                if (existing is null)
                {
                    return false;
                }

                existing.Title = todo.Title;
                existing.IsDone = todo.IsDone;
                return true;
            }
        }

        public bool Remove(int id)
        {
            lock (_lock)
            {
                var existing = _todos.FirstOrDefault(t => t.Id == id);
                if (existing is null)
                {
                    return false;
                }

                _todos.Remove(existing);
                return true;
            }
        }

        public Todo GetById(int id)
        {
            lock (_lock)
            {
                return _todos.FirstOrDefault(t => t.Id == id);
            }
        }

        public IReadOnlyList<Todo> GetAll()
        {
            lock (_lock)
            {
                return _todos
                    .OrderBy(t => t.IsDone)
                    .ThenByDescending(t => t.CreatedAt)
                    .ToList();
            }
        }
    }
}
