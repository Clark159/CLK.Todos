namespace CLK.Todos
{
    /// <summary>
    /// Domain 的入口物件：所有 Repository 都透過建構子注入到這裡，
    /// 外部一律透過 TodoContext 的屬性存取 Repository，不直接注入個別 Repository 介面。
    /// </summary>
    public class TodoContext
    {
        public TodoContext(ITodoRepository todos)
        {
            Todos = todos;
        }

        public ITodoRepository Todos { get; }
    }
}
