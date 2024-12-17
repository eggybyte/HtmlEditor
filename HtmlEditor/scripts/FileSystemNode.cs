using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.IO;

public class FileSystemNode : ITreeNode
{
    private const string BasePath = "files"; // Base path for all file paths

    /// <summary>
    /// Gets the name of the file or directory.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the relative file path.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets a value indicating whether this node represents a directory.
    /// </summary>
    public bool IsDirectory { get; }

    /// <summary>
    /// Gets a value indicating whether this file is open.
    /// </summary>
    public bool IsOpen { get; }

    /// <summary>
    /// Gets a value indicating whether this file has been modified.
    /// </summary>
    public bool IsDirty { get; }

    private List<ITreeNode> childrenList;

    /// <summary>
    /// Gets the children of this node.
    /// </summary>
    public IEnumerable<ITreeNode> Children => childrenList;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemNode"/> class.
    /// </summary>
    /// <param name="path">The full path of the file or directory.</param>
    /// <param name="openFiles">The list of open files used to mark file status.</param>
    public FileSystemNode(string path, List<Editor> openFiles)
    {
        // Convert the path to a relative path
        FilePath = Path.GetRelativePath(BasePath, Path.GetFullPath(path));
        Name = Path.GetFileName(FilePath);

        if (string.IsNullOrEmpty(Name))
        {
            Name = FilePath; // For the root directory
        }

        IsDirectory = Directory.Exists(Path.Combine(BasePath, FilePath));

        // Generate the full path for comparison
        var fullPath = Path.GetFullPath(Path.Combine(BasePath, FilePath));
        var openFile = openFiles.FirstOrDefault(f => f.FilePath == FilePath);

        IsOpen = !string.IsNullOrEmpty(openFile?.FilePath);
        IsDirty = IsOpen && (openFile?.IsDirty ?? false);

        // Debug output
        // Console.WriteLine($"{Path.GetFullPath(openFiles[0].FilePath)}");
        // Console.WriteLine($"[Debug] Constructing Node:");
        // Console.WriteLine($"  Path: {path}");
        // Console.WriteLine($"  FilePath (relative to BasePath): {FilePath}");
        // Console.WriteLine($"  IsDirectory: {IsDirectory}, IsOpen: {IsOpen}, IsDirty: {IsDirty}");

        childrenList = new List<ITreeNode>();

        if (IsDirectory)
        {
            try
            {
                // Get all subdirectories and files
                var directories = Directory.GetDirectories(fullPath).OrderBy(d => d);
                var files = Directory.GetFiles(fullPath).OrderBy(f => f);

                // Add existing subdirectories and files
                foreach (var directory in directories)
                {
                    childrenList.Add(new FileSystemNode(directory, openFiles));
                }

                foreach (var file in files)
                {
                    childrenList.Add(new FileSystemNode(file, openFiles));
                }
            }
            catch (Exception ex)
            {
                // If access is denied or other exceptions occur, set Children to empty and output the error
                Console.WriteLine($"[Debug] Error accessing directory {path}: {ex.Message}");
                childrenList = new List<ITreeNode>();
            }
        }
    }

    /// <summary>
    /// Gets the output content of the current node.
    /// </summary>
    /// <param name="showId">Whether to show the ID (not applicable in FileSystemNode, always returns Name).</param>
    /// <returns>The output string of the current node.</returns>
    public string GetOutputContent(bool showId)
    {
        if (IsDirectory)
        {
            return $"{Name}/";
        }
        else
        {
            // If the file is marked as modified, add '*'
            return IsDirty ? $"{Name}*" : Name;
        }
    }

    /// <summary>
    /// Finds a node with the specified path in the tree.
    /// </summary>
    /// <param name="path">The path to find (relative path).</param>
    /// <returns>The found FileSystemNode or null.</returns>
    public FileSystemNode FindNode(string path)
    {
        if (FilePath.Equals(path, StringComparison.OrdinalIgnoreCase))
            return this;

        if (!IsDirectory)
            return null;

        foreach (var child in Children)
        {
            if (child is FileSystemNode fsNode)
            {
                var result = fsNode.FindNode(path);
                if (result != null)
                    return result;
            }
        }

        return null;
    }

    /// <summary>
    /// Adds a child node.
    /// </summary>
    /// <param name="child">The child node to add.</param>
    public void AddChild(FileSystemNode child)
    {
        if (!childrenList.Any(c => (c as FileSystemNode)?.FilePath.Equals(child.FilePath, StringComparison.OrdinalIgnoreCase) == true))
        {
            childrenList.Add(child);
        }
    }
}