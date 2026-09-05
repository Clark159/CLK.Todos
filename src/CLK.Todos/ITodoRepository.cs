namespace CLK.Todos;

public interface ITodoRepository
{
    // Methods
    Todo Add(Todo todo);

    bool Update(Todo todo);

    bool Remove(Guid todoId);

    Todo? FindById(Guid todoId);

    IReadOnlyList<Todo> FindAll();
}
