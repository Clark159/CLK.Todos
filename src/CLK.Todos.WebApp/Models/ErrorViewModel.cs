namespace CLK.Todos.WebApp;

public class ErrorViewModel
{
    // Properties
    public string RequestId { get; set; } = string.Empty;

    public bool ShowRequestId
    {
        get { return !string.IsNullOrEmpty(RequestId); }
    }
}
