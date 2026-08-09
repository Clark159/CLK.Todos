namespace CLK.Todos.WebApp
{
    public class ErrorViewModel
    {
        // Properties
        public string RequestId { get; set; }

        public bool ShowRequestId
        {
            get { return string.IsNullOrEmpty(this.RequestId) == false; }
        }
    }
}
