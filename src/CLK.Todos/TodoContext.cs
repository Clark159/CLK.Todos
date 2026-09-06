namespace CLK.Todos;

public class TodoContext
{
    // Fields
    private readonly ITodoRepository _todoRepository;

    private readonly IUserRepository _userRepository;


    // Constructors
    public TodoContext(ITodoRepository todoRepository, IUserRepository userRepository)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(todoRepository);
        ArgumentNullException.ThrowIfNull(userRepository);

        // Default
        _todoRepository = todoRepository;
        _userRepository = userRepository;
    }


    // Properties
    public ITodoRepository TodoRepository
    {
        get { return _todoRepository; }
    }

    public IUserRepository UserRepository
    {
        get { return _userRepository; }
    }
}
