using LogFileReaderWEB.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LogFileReaderWEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly string logDirectory;

      
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            logDirectory = configuration["LoggingSettings:LogDirectory"]??"";

            if (string.IsNullOrWhiteSpace(logDirectory))
            {
                throw new ArgumentException("Log directory is not configured in appsettings.json");
            }

            // Optional: Ensure directory exists
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
                _logger.LogInformation($"Log directory created at {logDirectory}");
            }
        }
        [HttpGet]
        public IActionResult Index()
        {
            DateTime today = DateTime.Now;

            // Pass default values to ViewBag for pre-populating the form
            ViewBag.SelectedDate = today.ToString("MM/dd/yyyy");
            ViewBag.SearchValue = string.Empty;
            return View(new List<string>());
        }

        [HttpPost]
        public IActionResult Index(DateTime datevalue, string searchValue)
        {
            ViewBag.SelectedDate = datevalue.ToString("yyyy-MM-dd");
            ViewBag.SearchValue = searchValue;

            // Construct file path
            string fileName = $"log-{datevalue:yyyyMMdd}.txt";
            string filePath = Path.Combine(logDirectory, fileName);

            // Check file existence
            if (!System.IO.File.Exists(filePath))
            {
                ViewBag.Message = $"No log file found for {datevalue:dd-MM-yyyy}.";
                return View(new List<string>());
            }

            var allLines = System.IO.File.ReadAllLines(filePath).ToList();
            var matchedLines = new List<string>();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                string searchTerm = searchValue.Trim();

                // Get only lines that contain the full search term
                matchedLines = allLines
                    .Where(line => line.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            else
            {
                // If no search term given, return everything
                matchedLines = allLines;
            }

            return View(matchedLines);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
