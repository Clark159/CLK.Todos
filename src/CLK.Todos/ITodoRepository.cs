namespace CLK.Todos
{
    public interface ITodoRepository
    {
        // Methods
        Todo Add(Todo todo);

        bool Update(Todo todo);

        bool Remove(Guid id);

        Todo FindById(Guid id);

        IReadOnlyList<Todo> FindAll();
    }
}
