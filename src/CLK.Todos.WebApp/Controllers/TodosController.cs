using CLK.Todos;
using Microsoft.AspNetCore.Mvc;

namespace CLK.Todos.WebApp;

public class TodosController : Controller
{
    // Fields
    private readonly TodoContext _todoContext;


    // Constructors
    public TodosController(TodoContext todoContext)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(todoContext);

        // Default
        _todoContext = todoContext;
    }


    // Methods
    // GET: /Todos
    public IActionResult Index()
    {
        // Search
        var todos = _todoContext.TodoRepository.FindAll();

        // Return
        return View(todos);
    }

    // GET: /Todos/Create
    public IActionResult Create()
    {
        // Return
        return View();
    }

    // POST: /Todos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Title")] Todo todo = null)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(todo);
        if (!ModelState.IsValid) return View(todo);

        // Execute
        _todoContext.TodoRepository.Add(todo);

        // Return
        return RedirectToAction(nameof(Index));
    }

    // GET: /Todos/Edit/{todoId}
    public IActionResult Edit(Guid todoId)
    {
        // Search
        var todo = _todoContext.TodoRepository.FindById(todoId);
        if (todo is null) return NotFound();

        // Return
        return View(todo);
    }

    // POST: /Todos/Edit/{todoId}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Guid todoId, [Bind("TodoId,Title,IsDone")] Todo todo = null)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(todo);
        if (todoId != todo.TodoId) return View(todo);
        if (!ModelState.IsValid) return View(todo);

        // Execute
        _todoContext.TodoRepository.Update(todo);

        // Return
        return RedirectToAction(nameof(Index));
    }

    // GET: /Todos/Delete/{todoId}
    public IActionResult Delete(Guid todoId)
    {
        // Search
        var todo = _todoContext.TodoRepository.FindById(todoId);
        if (todo is null) return NotFound();

        // Return
        return View(todo);
    }

    // POST: /Todos/Delete/{todoId}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(Guid todoId)
    {
        // Execute
        _todoContext.TodoRepository.Remove(todoId);

        // Return
        return RedirectToAction(nameof(Index));
    }

    // POST: /Todos/Toggle/{todoId}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Toggle(Guid todoId)
    {
        // Search
        var todo = _todoContext.TodoRepository.FindById(todoId);
        if (todo is null) return NotFound();

        // Execute
        todo.ToggleDone();
        _todoContext.TodoRepository.Update(todo);

        // Return
        return RedirectToAction(nameof(Index));
    }
}
