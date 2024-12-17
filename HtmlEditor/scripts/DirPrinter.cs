/// <summary>
/// A utility class for printing directory structures in various formats.
/// </summary>
public static class DirPrinter
{
    /// <summary>
    /// The folder name used as a default subdirectory for operations.
    /// </summary>
    private const string Folder = "files";

    /// <summary>
    /// Combines the specified root directory with the default folder name to get the full path.
    /// </summary>
    /// <param name="rootDir">The root directory path.</param>
    /// <returns>The combined full path of the root directory and the default folder.</returns>
    private static string GetFullPath(string rootDir)
    {
        return Path.Combine(rootDir, Folder);
    }

    /// <summary>
    /// Prints a tree-like representation of the directory structure.
    /// </summary>
    /// <param name="rootDir">The root directory to start printing from.</param>
    /// <param name="openFiles">A list of <see cref="Editor"/> objects representing open files in the directory.</param>
    /// <returns>A string representation of the directory structure in a tree format.</returns>
    public static string PrintDirTree(string rootDir, List<Editor> openFiles)
    {
        string rootFullPath = GetFullPath(rootDir);
        var rootNode = new FileSystemNode(rootFullPath, openFiles);
        return TreePrinter.Print(rootNode, showId: false, markError: false, useSymbols: true, indentSize: 2);
    }

    /// <summary>
    /// Prints a directory structure with customizable indentation.
    /// </summary>
    /// <param name="rootDir">The root directory to start printing from.</param>
    /// <param name="openFiles">A list of <see cref="Editor"/> objects representing open files in the directory.</param>
    /// <param name="indent">The number of spaces used for indentation. Defaults to 4.</param>
    /// <returns>A string representation of the directory structure with the specified indentation.</returns>
    public static string PrintDirIndent(string rootDir, List<Editor> openFiles, int indent = 4)
    {
        string rootFullPath = GetFullPath(rootDir);
        var rootNode = new FileSystemNode(rootFullPath, openFiles);
        return TreePrinter.Print(rootNode, showId: false, markError: false, useSymbols: false, indentSize: indent);
    }
}
