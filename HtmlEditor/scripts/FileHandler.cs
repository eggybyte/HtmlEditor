using System;
using System.IO;

/// <summary>
/// Provides utility methods for file handling, including reading, writing, and ensuring directory structures.
/// </summary>
public static class FileHandler
{
    /// <summary>
    /// The default folder used as the base directory for file operations.
    /// </summary>
    private const string Folder = "files";

    /// <summary>
    /// Checks whether a file exists in the default folder.
    /// </summary>
    /// <param name="filePath">The relative path of the file to check.</param>
    /// <returns><c>true</c> if the file exists; otherwise, <c>false</c>.</returns>
    public static bool FileExists(string filePath)
    {
        string fullPath = GetFullPath(filePath);
        return File.Exists(fullPath);
    }

    /// <summary>
    /// Reads all text content from a file in the default folder.
    /// </summary>
    /// <param name="filePath">The relative path of the file to read.</param>
    /// <returns>The content of the file as a string.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
    public static string ReadAllText(string filePath)
    {
        if (!FileExists(filePath))
        {
            throw new FileNotFoundException($"File '{filePath}' does not exist.");
        }
        string fullPath = GetFullPath(filePath);
        return File.ReadAllText(fullPath);
    }

    /// <summary>
    /// Writes the specified text content to a file in the default folder.
    /// </summary>
    /// <param name="filePath">The relative path of the file to write to.</param>
    /// <param name="content">The text content to write.</param>
    public static void WriteAllText(string filePath, string content)
    {
        string fullPath = GetFullPath(filePath);
        EnsureDirectoryExists(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content);
    }

    /// <summary>
    /// Reads all lines of text from a file in the default folder.
    /// </summary>
    /// <param name="filePath">The relative path of the file to read.</param>
    /// <returns>An array of strings containing the lines of the file.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
    public static string[] ReadAllLines(string filePath)
    {
        if (!FileExists(filePath))
        {
            throw new FileNotFoundException($"File '{filePath}' does not exist.");
        }
        string fullPath = GetFullPath(filePath);
        return File.ReadAllLines(fullPath);
    }

    /// <summary>
    /// Writes an array of lines to a file in the default folder.
    /// </summary>
    /// <param name="filePath">The relative path of the file to write to.</param>
    /// <param name="lines">The array of strings to write to the file.</param>
    public static void WriteAllLines(string filePath, string[] lines)
    {
        string fullPath = GetFullPath(filePath);
        EnsureDirectoryExists(Path.GetDirectoryName(fullPath)!);
        File.WriteAllLines(fullPath, lines);
    }

    /// <summary>
    /// Combines the default folder with the specified relative file path to create a full path.
    /// </summary>
    /// <param name="filePath">The relative path of the file.</param>
    /// <returns>The full path of the file.</returns>
    private static string GetFullPath(string filePath)
    {
        return Path.Combine(Folder, filePath);
    }

    /// <summary>
    /// Ensures that the specified directory exists. If it does not exist, it is created.
    /// </summary>
    /// <param name="folder">The directory path to check or create. Defaults to the default folder.</param>
    public static void EnsureDirectoryExists(string folder = Folder)
    {
        if (string.IsNullOrEmpty(folder))
        {
            return;
        }
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
    }
}
