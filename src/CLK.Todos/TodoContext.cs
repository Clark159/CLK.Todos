namespace CLK.Todos
{
    public class TodoContext
    {
        // Fields
        private readonly ITodoRepository _todoRepository;


        // Constructors
        public TodoContext(ITodoRepository todoRepository)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(todoRepository);

            #endregion

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
