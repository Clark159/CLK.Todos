using CLK.Todos.Entities;
using CLK.Todos.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CLK.Todos.Web.Controllers;

public class TodosController : Controller
{
    private readonly ITodoRepository _todoRepository;

    public TodosController(ITodoRepository todoRepository)
    {
        _todoRepository = todoRepository;
    }

    // GET: /Todos
    public IActionResult Index()
    {
        var todos = _todoRepository.GetAll();
        return View(todos);
    }

    // GET: /Todos/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Todos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Title")] Todo todo)
    {
        if (!ModelState.IsValid)
        {
            return View(todo);
        }

        _todoRepository.Add(todo);
        return RedirectToAction(nameof(Index));
    }

    // GET: /Todos/Edit/5
    public IActionResult Edit(int id)
    {
        var todo = _todoRepository.GetById(id);
        if (todo is null)
        {
            return NotFound();
        }

        return View(todo);
    }

    // POST: /Todos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("Id,Title,IsDone")] Todo todo)
    {
        if (id != todo.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(todo);
        }

        _todoRepository.Update(todo);
        return RedirectToAction(nameof(Index));
    }

    // GET: /Todos/Delete/5
    public IActionResult Delete(int id)
    {
        var todo = _todoRepository.GetById(id);
        if (todo is null)
        {
            return NotFound();
        }

        return View(todo);
    }

    // POST: /Todos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        _todoRepository.Delete(id);
        return RedirectToAction(nameof(Index));
    }

    // POST: /Todos/Toggle/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Toggle(int id)
    {
        _todoRepository.ToggleDone(id);
        return RedirectToAction(nameof(Index));
    }
}
