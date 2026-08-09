using CLK.Todos.Web.Models;

namespace CLK.Todos.Web.Services;

public interface ITodoStore
{
    IReadOnlyList<Todo> GetAll();

    Todo? GetById(int id);

    Todo Add(Todo todo);

    bool Update(Todo todo);

    bool Delete(int id);

    bool ToggleDone(int id);
}
