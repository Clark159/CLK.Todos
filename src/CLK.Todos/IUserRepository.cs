namespace CLK.Todos;

public interface IUserRepository
{
    // Methods
    void Add(User user);

    void Update(User user);

    void Remove(Guid userId);

    User? FindById(Guid userId);

    IReadOnlyList<User> FindAll();
}
