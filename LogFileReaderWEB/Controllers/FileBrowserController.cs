using LogFileReaderWEB.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogFileReaderWEB.Controllers
{
    public class FileBrowserController : Controller
    {
        private readonly string _rootPath;

        public FileBrowserController(IConfiguration configuration)
        {
            _rootPath = configuration["LoggingSettings:LogDirectory"] ?? "";

            if (string.IsNullOrWhiteSpace(_rootPath))
            {
                throw new ArgumentException("Log directory is not configured in appsettings.json");
            }
            
            if (!Directory.Exists(_rootPath))
            {
                Directory.CreateDirectory(_rootPath);

            }
        }

        [HttpGet]
        public IActionResult Index(string? path)
        {
            
            string currentPath = string.IsNullOrEmpty(path) ? _rootPath : path;           
            if (!Directory.Exists(currentPath))
                return NotFound("Directory not found.");
           
            var directories = Directory.GetDirectories(currentPath)
                .Select(d => new FileItem
                {
                    Name = Path.GetFileName(d),
                    Path = d,
                    IsDirectory = true,
                    Modified = Directory.GetLastWriteTime(d).ToString("yyyy-MM-dd HH:mm")
                })
                .ToList();

            
            var files = Directory.GetFiles(currentPath)
                .Select(f => new FileItem
                {
                    Name = Path.GetFileName(f),
                    Path = f,
                    IsDirectory = false,
                    Size = GetReadableFileSize(f),
                    Modified = System.IO.File.GetLastWriteTime(f).ToString("yyyy-MM-dd HH:mm")
                })
                .ToList();

           
            string? parentPath = null;
            if (!string.Equals(currentPath, _rootPath, StringComparison.OrdinalIgnoreCase))
            {
                parentPath = Path.GetDirectoryName(currentPath);
               
                parentPath ??= _rootPath;
            }

           
            ViewBag.CurrentPath = currentPath;
            ViewBag.RootPath = _rootPath;
            ViewBag.ParentPath = parentPath;

            
            return View((directories.AsEnumerable(), files.AsEnumerable()));
        }


        string GetReadableFileSize(string filePath)
        {
            var length = new FileInfo(filePath).Length; // size in bytes
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = length;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }



        [HttpGet]
        public IActionResult FileView(string path, string? keyword)
        {
            if (!System.IO.File.Exists(path))
                return NotFound("File not found.");

            
            var allLines = System.IO.File.ReadAllLines(path);

           
            IEnumerable<string> filteredLines = allLines;
            if (!string.IsNullOrEmpty(keyword))
            {
                filteredLines = allLines
                    .Where(l => l.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.FilePath = path;
            ViewBag.FileName = System.IO.Path.GetFileName(path);
            ViewBag.ParentPath = System.IO.Path.GetDirectoryName(path);
            ViewBag.Keyword = keyword;

            return View(filteredLines);
        }

        [HttpGet]
        public IActionResult Search(string path, string keyword)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return NotFound("Directory not found.");

           
            var directories = Directory.GetDirectories(path)
                .Select(d => new FileItem
                {
                    Name = Path.GetFileName(d),
                    Path = d,
                    IsDirectory = true,
                    Modified = Directory.GetLastWriteTime(d).ToString("yyyy-MM-dd HH:mm")
                })
                .Where(d => string.IsNullOrEmpty(keyword) || d.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();

           
            var files = Directory.GetFiles(path)
                .Select(f => new FileItem
                {
                    Name = Path.GetFileName(f),
                    Path = f,
                    IsDirectory = false,
                    Size = GetReadableFileSize(f),
                    Modified = System.IO.File.GetLastWriteTime(f).ToString("yyyy-MM-dd HH:mm")
                })
                .Where(f => string.IsNullOrEmpty(keyword) || f.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();

           
            string? parentPath = null;
            if (!string.Equals(path, _rootPath, StringComparison.OrdinalIgnoreCase))
            {
                parentPath = Path.GetDirectoryName(path) ?? _rootPath;
            }

            ViewBag.CurrentPath = path;
            ViewBag.ParentPath = parentPath;
            ViewBag.RootPath = _rootPath;

            return View("Index", (directories.AsEnumerable(), files.AsEnumerable()));
        }
    }
}
