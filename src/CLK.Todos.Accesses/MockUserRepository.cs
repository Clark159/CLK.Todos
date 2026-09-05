using CLK.Todos;

namespace CLK.Todos.Accesses;

public class MockUserRepository : IUserRepository
{
    // Fields
    private readonly object _lock = new();

    private readonly List<User> _users = new();


    // Methods
    public void Add(User user)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(user);

        // Lock
        lock (_lock)
        {
            // Execute
            user.CreateTime = DateTime.UtcNow;
            user.UpdateTime = DateTime.UtcNow;
            _users.Add(user);
        }
    }

    public void Update(User user)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(user);

        // Lock
        lock (_lock)
        {
            // Search
            var entity = _users.FirstOrDefault(u => u.UserId == user.UserId);
            if (entity is null) throw new KeyNotFoundException($"User not found: {user.UserId}");

            // Execute
            entity.Name = user.Name;
            entity.Email = user.Email;
            entity.IsActive = user.IsActive;
            entity.UpdateTime = DateTime.UtcNow;
        }
    }

    public void Remove(Guid userId)
    {
        // Lock
        lock (_lock)
        {
            // Search
            var entity = _users.FirstOrDefault(u => u.UserId == userId);
            if (entity is null) throw new KeyNotFoundException($"User not found: {userId}");

            // Execute
            _users.Remove(entity);
        }
    }

    public User? FindById(Guid userId)
    {
        // Lock
        lock (_lock)
        {
            // Return
            return _users.FirstOrDefault(u => u.UserId == userId);
        }
    }

    public IReadOnlyList<User> FindAll()
    {
        // Lock
        lock (_lock)
        {
            // Return
            return _users
                .OrderByDescending(u => u.IsActive)
                .ThenByDescending(u => u.CreateTime)
                .ToList();
        }
    }
}
