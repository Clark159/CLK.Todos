using CLK.Todos.Web.Models;
using CLK.Todos.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CLK.Todos.Web.Controllers;

public class TodosController : Controller
{
    private readonly ITodoStore _todoStore;

    public TodosController(ITodoStore todoStore)
    {
        _todoStore = todoStore;
    }

    // GET: /Todos
    public IActionResult Index()
    {
        var todos = _todoStore.GetAll();
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

        _todoStore.Add(todo);
        return RedirectToAction(nameof(Index));
    }

    // GET: /Todos/Edit/5
    public IActionResult Edit(int id)
    {
        var todo = _todoStore.GetById(id);
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

        _todoStore.Update(todo);
        return RedirectToAction(nameof(Index));
    }

    // GET: /Todos/Delete/5
    public IActionResult Delete(int id)
    {
        var todo = _todoStore.GetById(id);
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
        _todoStore.Delete(id);
        return RedirectToAction(nameof(Index));
    }

    // POST: /Todos/Toggle/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Toggle(int id)
    {
        _todoStore.ToggleDone(id);
        return RedirectToAction(nameof(Index));
    }
}
