namespace CLK.Todos
{
    public interface ITodoRepository
    {
        // Methods
        Todo Add(Todo todo);

        bool Update(Todo todo);

        bool Remove(int id);

        Todo GetById(int id);

        IReadOnlyList<Todo> GetAll();
    }
}
