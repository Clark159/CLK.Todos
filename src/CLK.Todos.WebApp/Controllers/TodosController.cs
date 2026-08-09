// Imports
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
            #region Contracts

            ArgumentNullException.ThrowIfNull(todoContext);

            #endregion

            _todoContext = todoContext;
        }


        // Methods
        // GET: /Todos
        public IActionResult Index()
        {
            var todos = _todoContext.TodoRepository.GetAll();
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
        public IActionResult Create([Bind("Title")] Todo todo = null)
        {
            #region Contracts

            if (todo is null || !ModelState.IsValid)
            {
                return View(todo);
            }

            #endregion

            _todoContext.TodoRepository.Add(todo);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Todos/Edit/5
        public IActionResult Edit(int id)
        {
            var todo = _todoContext.TodoRepository.GetById(id);
            if (todo is null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // POST: /Todos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,Title,IsDone")] Todo todo = null)
        {
            #region Contracts

            if (todo is null || id != todo.Id || !ModelState.IsValid)
            {
                return View(todo);
            }

            #endregion

            _todoContext.TodoRepository.Update(todo);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Todos/Delete/5
        public IActionResult Delete(int id)
        {
            var todo = _todoContext.TodoRepository.GetById(id);
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
            _todoContext.TodoRepository.Remove(id);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Todos/Toggle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Toggle(int id)
        {
            var todo = _todoContext.TodoRepository.GetById(id);
            if (todo is null)
            {
                return NotFound();
            }

            todo.ToggleDone();
            _todoContext.TodoRepository.Update(todo);
            return RedirectToAction(nameof(Index));
        }
    }
}
