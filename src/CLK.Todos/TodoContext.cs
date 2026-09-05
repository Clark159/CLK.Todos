namespace CLK.Todos;

public class TodoContext
{
    // Fields
    private readonly ITodoRepository _todoRepository;

    private readonly IUserRepository _userRepository;

    private readonly IMeetingRepository _meetingRepository;


    // Constructors
    public TodoContext(ITodoRepository todoRepository, IUserRepository userRepository, IMeetingRepository meetingRepository)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(todoRepository);
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(meetingRepository);

        // Default
        _todoRepository = todoRepository;
        _userRepository = userRepository;
        _meetingRepository = meetingRepository;
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

    public IMeetingRepository MeetingRepository
    {
        get { return _meetingRepository; }
    }
}
