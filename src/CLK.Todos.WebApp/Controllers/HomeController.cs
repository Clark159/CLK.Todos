using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CLK.Todos.WebApp
{
    public class HomeController : Controller
    {
        // Fields
        private readonly ILogger<HomeController> _logger;


        // Constructors
        public HomeController(ILogger<HomeController> logger)
        {
            // Contracts
            ArgumentNullException.ThrowIfNull(logger);

            // Default
            _logger = logger;
        }


        // Methods
        public IActionResult Index()
        {
            // Result
            return View();
        }

        public IActionResult Privacy()
        {
            // Result
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Result
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }
    }
}
