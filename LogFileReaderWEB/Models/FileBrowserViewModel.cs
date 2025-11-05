using Microsoft.AspNetCore.Mvc.Filters;

namespace LogFileReaderWEB.Models
{
    public class FileBrowserViewModel
    {
        public List<DirectoryItem> Directories { get; set; } = new();
        public List<FileItem> Files { get; set; } = new();
        public string? CurrentPath { get; set; }
        public string? ParentPath { get; set; }
    }

    public class DirectoryItem
    {
        public string? Name { get; set; }
        public string? FullPath { get; set; }
    }

    public class FileItem
    {
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? Size { get; set; }
        public string? Modified { get; set; }
        public bool IsDirectory { get; set; }
    }
}
