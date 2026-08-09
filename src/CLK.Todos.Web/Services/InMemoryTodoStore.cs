using CLK.Todos.Web.Models;

namespace CLK.Todos.Web.Services;

/// <summary>
/// 最簡單的記憶體內資料存放：用 List 存資料，重啟程式就會清空。
/// 用 lock 保護，避免多個請求同時新增/修改造成資料錯亂。
/// 之後若要換成資料庫，只要另外實作 ITodoStore 就好，Controller 不用改。
/// </summary>
public class InMemoryTodoStore : ITodoStore
{
    private readonly List<Todo> _todos = new();
    private readonly object _lock = new();
    private int _nextId = 1;

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

    public Todo? GetById(int id)
    {
        lock (_lock)
        {
            return _todos.FirstOrDefault(t => t.Id == id);
        }
    }

    public Todo Add(Todo todo)
    {
        lock (_lock)
        {
            todo.Id = _nextId++;
            _todos.Add(todo);
            return todo;
        }
    }

    public bool Update(Todo todo)
    {
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

    public bool Delete(int id)
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

    public bool ToggleDone(int id)
    {
        lock (_lock)
        {
            var existing = _todos.FirstOrDefault(t => t.Id == id);
            if (existing is null)
            {
                return false;
            }

            existing.IsDone = !existing.IsDone;
            return true;
        }
    }
}
