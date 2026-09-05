namespace CLK.Todos
{
    public class TodoContext
    {
        // Fields
        private readonly ITodoRepository _todoRepository;


        // Constructors
        public TodoContext(ITodoRepository todoRepository)
        {
            // Contracts
            ArgumentNullException.ThrowIfNull(todoRepository);

            // Default
            _todoRepository = todoRepository;
        }


        // Properties
        public ITodoRepository TodoRepository
        {
            get { return _todoRepository; }
        }
    }
}
