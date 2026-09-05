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
            // Search
            var todos = _todoContext.TodoRepository.FindAll();

            // Result
            return View(todos);
        }

        // GET: /Todos/Create
        public IActionResult Create()
        {
            // Result
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

            // Execute
            _todoContext.TodoRepository.Add(todo);

            // Result
            return RedirectToAction(nameof(Index));
        }

        // GET: /Todos/Edit/{id}
        public IActionResult Edit(Guid id)
        {
            // Search
            var todo = _todoContext.TodoRepository.FindById(id);
            if (todo is null) return NotFound();

            // Result
            return View(todo);
        }

        // POST: /Todos/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, [Bind("Id,Title,IsDone")] Todo todo = null)
        {
            // Contracts
            ArgumentNullException.ThrowIfNull(todo);
            if (id != todo.Id) return View(todo);
            if (this.ModelState.IsValid == false) return View(todo);

            // Execute
            _todoContext.TodoRepository.Update(todo);

            // Result
            return RedirectToAction(nameof(Index));
        }

        // GET: /Todos/Delete/{id}
        public IActionResult Delete(Guid id)
        {
            // Search
            var todo = _todoContext.TodoRepository.FindById(id);
            if (todo is null) return NotFound();

            // Result
            return View(todo);
        }

        // POST: /Todos/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            // Execute
            _todoContext.TodoRepository.Remove(id);

            // Result
            return RedirectToAction(nameof(Index));
        }

        // POST: /Todos/Toggle/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Toggle(Guid id)
        {
            // Search
            var todo = _todoContext.TodoRepository.FindById(id);
            if (todo is null) return NotFound();

            // Execute
            todo.ToggleDone();
            _todoContext.TodoRepository.Update(todo);

            // Result
            return RedirectToAction(nameof(Index));
        }
    }
}
