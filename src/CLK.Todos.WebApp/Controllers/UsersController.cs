using CLK.Todos;
using Microsoft.AspNetCore.Mvc;

namespace CLK.Todos.WebApp;

public class UsersController : Controller
{
    // Fields
    private readonly TodoContext _todoContext;


    // Constructors
    public UsersController(TodoContext todoContext)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(todoContext);

        // Default
        _todoContext = todoContext;
    }


    // Methods
    // GET: /Users
    public IActionResult Index()
    {
        // Search
        var users = _todoContext.UserRepository.FindAll();

        // Return
        return View(users);
    }

    // GET: /Users/Create
    public IActionResult Create()
    {
        // Return
        return View();
    }

    // POST: /Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Name,Email")] User? user = null)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(user);
        if (!ModelState.IsValid) return View(user);

        // Execute
        _todoContext.UserRepository.Add(user);

        // Return
        return RedirectToAction(nameof(Index));
    }

    // GET: /Users/Edit/{userId}
    public IActionResult Edit(Guid userId)
    {
        // Search
        var user = _todoContext.UserRepository.FindById(userId);
        if (user is null) return NotFound();

        // Return
        return View(user);
    }

    // POST: /Users/Edit/{userId}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Guid userId, [Bind("UserId,Name,Email,IsActive")] User? user = null)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(user);
        if (userId != user.UserId) return View(user);
        if (!ModelState.IsValid) return View(user);

        // Execute
        _todoContext.UserRepository.Update(user);

        // Return
        return RedirectToAction(nameof(Index));
    }

    // GET: /Users/Delete/{userId}
    public IActionResult Delete(Guid userId)
    {
        // Search
        var user = _todoContext.UserRepository.FindById(userId);
        if (user is null) return NotFound();

        // Return
        return View(user);
    }

    // POST: /Users/Delete/{userId}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(Guid userId)
    {
        // Execute
        _todoContext.UserRepository.Remove(userId);

        // Return
        return RedirectToAction(nameof(Index));
    }

    // POST: /Users/Toggle/{userId}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Toggle(Guid userId)
    {
        // Search
        var user = _todoContext.UserRepository.FindById(userId);
        if (user is null) return NotFound();

        // Execute
        user.ToggleActive();
        _todoContext.UserRepository.Update(user);

        // Return
        return RedirectToAction(nameof(Index));
    }
}
