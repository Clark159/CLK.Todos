namespace CLK.Todos;

public interface ITodoRepository
{
    // Methods
    void Add(Todo todo);

    void Update(Todo todo);

    void Remove(Guid todoId);

    Todo? FindById(Guid todoId);

    IReadOnlyList<Todo> FindAll();
}
