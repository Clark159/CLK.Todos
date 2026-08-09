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
            #region Contracts

            ArgumentNullException.ThrowIfNull(logger);

            #endregion

            // Default
            _logger = logger;
        }


        // Methods
        public IActionResult Index()
        {
            // Return
            return View();
        }

        public IActionResult Privacy()
        {
            // Return
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Return
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }
    }
}
