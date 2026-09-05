using CLK.Todos;
using Microsoft.AspNetCore.Mvc;

namespace CLK.Todos.WebApp
{
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
            // FindAll
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
            if (this.ModelState.IsValid == false) return View(todo);

            // Add
            _todoContext.TodoRepository.Add(todo);

            // Return
            return RedirectToAction(nameof(Index));
        }

        // GET: /Todos/Edit/5
        public IActionResult Edit(int id)
        {
            // FindById
            var todo = _todoContext.TodoRepository.FindById(id);
            if (todo is null)
            {
                return NotFound();
            }

            // Return
            return View(todo);
        }

        // POST: /Todos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,Title,IsDone")] Todo todo = null)
        {
            // Contracts
            ArgumentNullException.ThrowIfNull(todo);
            if (id != todo.Id) return View(todo);
            if (this.ModelState.IsValid == false) return View(todo);

            // Update
            _todoContext.TodoRepository.Update(todo);

            // Return
            return RedirectToAction(nameof(Index));
        }

        // GET: /Todos/Delete/5
        public IActionResult Delete(int id)
        {
            // FindById
            var todo = _todoContext.TodoRepository.FindById(id);
            if (todo is null)
            {
                return NotFound();
            }

            // Return
            return View(todo);
        }

        // POST: /Todos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // Remove
            _todoContext.TodoRepository.Remove(id);

            // Return
            return RedirectToAction(nameof(Index));
        }

        // POST: /Todos/Toggle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Toggle(int id)
        {
            // FindById
            var todo = _todoContext.TodoRepository.FindById(id);
            if (todo is null)
            {
                return NotFound();
            }

            // ToggleDone
            todo.ToggleDone();
            _todoContext.TodoRepository.Update(todo);

            // Return
            return RedirectToAction(nameof(Index));
        }
    }
}
