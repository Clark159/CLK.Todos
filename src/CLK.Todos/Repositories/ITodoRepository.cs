using CLK.Todos.Entities;

namespace CLK.Todos.Repositories;

public interface ITodoRepository
{
    IReadOnlyList<Todo> GetAll();

    Todo? GetById(int id);

    Todo Add(Todo todo);

    bool Update(Todo todo);

    bool Delete(int id);

    bool ToggleDone(int id);
}
