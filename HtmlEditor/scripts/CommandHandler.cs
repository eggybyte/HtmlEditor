using System.Text.RegularExpressions;


/// <summary>
/// Represents a handler for executing commands in the HTML editor.
/// </summary>
public class CommandHandler
{
    /// <summary>
    /// The session associated with this command handler.
    /// </summary>
    private readonly Session session;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandHandler"/> class.
    /// </summary>
    /// <param name="session">The session to associate with this command handler.</param>
    public CommandHandler(Session session)
    {
        this.session = session;
    }

    /// <summary>
    /// Executes a command asynchronously.
    /// </summary>
    /// <param name="input">The input command string.</param>
    /// <returns>A task that represents the asynchronous operation. The result is a string containing the command execution result.</returns>
    public async Task<string> ExecuteCommandAsync(string input)
    {
        var parts = input.Split(' ', 2);
        string command = parts[0].ToLower();
        string arguments = parts.Length > 1 ? parts[1] : "";

        try
        {
            switch (command)
            {
                case "help":
                    return "Commands: load <file>, save, close, editor-list, edit <file>, exit, print-tree, showid true/false, dir-tree, dir-indent [indent], insert <...>, append <...>, edit-id <...>, edit-text <...>, delete <...>, undo, redo";

                case "load":
                    return LoadCommand(arguments);

                case "save":
                    return SaveCommand();

                case "close":
                    return CloseCommand();

                case "editor-list":
                    return EditorListCommand();

                case "edit":
                    return EditCommand(arguments);

                case "exit":
                    return ExitCommand();

                case "print-tree":
                    return await PrintTreeCommand();

                case "print-indent":
                    return await PrintIndentCommand(arguments);

                case "showid":
                    return ShowIdCommand(arguments);

                case "dir-tree":
                    return DirTreeCommand();

                case "dir-indent":
                    return DirIndentCommand(arguments);

                // Commands that operate on the content of the active editor
                case "insert":
                    ValidateActiveEditor();
                    var insertArgs = SplitArguments(arguments);
                    session.ActiveEditor.HtmlEditor.Insert(insertArgs[0], insertArgs[1], insertArgs[2], insertArgs.Length > 3 ? insertArgs[3] : "");
                    session.ActiveEditor.IsDirty = true;
                    return $"Element '{insertArgs[1]}' inserted before '{insertArgs[2]}'.";

                case "append":
                    ValidateActiveEditor();
                    var appendArgs = SplitArguments(arguments);
                    session.ActiveEditor.HtmlEditor.Append(appendArgs[0], appendArgs[1], appendArgs[2], appendArgs.Length > 3 ? appendArgs[3] : "");
                    session.ActiveEditor.IsDirty = true;
                    return $"Element '{appendArgs[1]}' appended to '{appendArgs[2]}'.";

                case "edit-id":
                    ValidateActiveEditor();
                    var editIdArgs = SplitArguments(arguments);
                    session.ActiveEditor.HtmlEditor.EditId(editIdArgs[0], editIdArgs[1]);
                    session.ActiveEditor.IsDirty = true;
                    return "ID edited.";

                case "edit-text":
                    ValidateActiveEditor();
                    var editTextArgs = SplitArguments(arguments);
                    session.ActiveEditor.HtmlEditor.EditText(editTextArgs[0], editTextArgs.Length > 1 ? editTextArgs[1] : "");
                    session.ActiveEditor.IsDirty = true;
                    return $"Text of element '{editTextArgs[0]}' updated.";

                case "delete":
                    ValidateActiveEditor();
                    session.ActiveEditor.HtmlEditor.Delete(arguments);
                    session.ActiveEditor.IsDirty = true;
                    return $"Element '{arguments}' deleted.";

                case "undo":
                    ValidateActiveEditor();
                    session.ActiveEditor.HtmlEditor.Undo();
                    session.ActiveEditor.IsDirty = true;
                    return "Undo completed.";

                case "redo":
                    ValidateActiveEditor();
                    session.ActiveEditor.HtmlEditor.Redo();
                    session.ActiveEditor.IsDirty = true;
                    return "Redo completed.";

                default:
                    return $"Unknown command: {command}";
            }
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Loads a file into the editor.
    /// </summary>
    /// <param name="filepath">The path to the file to load.</param>
    /// <returns>A message indicating the result of the operation.</returns>
    private string LoadCommand(string filepath)
    {
        if (string.IsNullOrWhiteSpace(filepath)) return "Error: no filepath provided.";
        // Check if already loaded
        if (session.GetEditorByFilePath(filepath) != null)
        {
            return "Error: file already loaded.";
        }
        var editor = new Editor(filepath);
        session.AddEditor(editor);
        return $"File '{filepath}' loaded. Active editor switched.";
    }

    /// <summary>
    /// Saves the active editor's file.
    /// </summary>
    /// <returns>A message indicating the result of the operation.</returns>
    private string SaveCommand()
    {
        ValidateActiveEditor();
        session.ActiveEditor.Save();
        return "File saved.";
    }

    /// <summary>
    /// Tracks close requests for editors.
    /// </summary>
    private readonly Dictionary<string, bool> closeRequestTracker = new();

    /// <summary>
    /// Closes the active editor.
    /// </summary>
    /// <returns>A message indicating the result of the operation.</returns>
    private string CloseCommand()
    {
        ValidateActiveEditor();
        var editor = session.ActiveEditor;

        // Check if it is the first close request
        if (editor.IsDirty)
        {
            if (!closeRequestTracker.ContainsKey(editor.FilePath))
            {
                // First request, return confirmation prompt, and record request status
                closeRequestTracker[editor.FilePath] = true;
                return $"File '{editor.FilePath}' not saved. Are you sure you want to close it? (Call 'close' again to confirm)";
            }

            // Second call, clear status and continue close operation
            closeRequestTracker.Remove(editor.FilePath);
        }

        // Close the editor
        session.RemoveEditor(editor);
        return $"Closed '{editor.FilePath}'.";
    }

    /// <summary>
    /// Lists all open editors.
    /// </summary>
    /// <returns>A list of open editors.</returns>
    private string EditorListCommand()
    {
        if (session.Editors.Count == 0) return "No editors open.";
        var result = "";
        foreach (var e in session.Editors)
        {
            var prefix = (session.ActiveEditor == e) ? ">" : " ";
            var suffix = e.IsDirty ? "*" : "";
            result += $"{prefix} {e.FilePath}{suffix}\n";
        }
        return result.TrimEnd();
    }

    /// <summary>
    /// Switches the active editor to the specified file.
    /// </summary>
    /// <param name="filepath">The path to the file to switch to.</param>
    /// <returns>A message indicating the result of the operation.</returns>
    private string EditCommand(string filepath)
    {
        if (string.IsNullOrWhiteSpace(filepath)) return "Error: no filepath provided.";
        var editor = session.GetEditorByFilePath(filepath);
        if (editor == null) return "Error: editor not found for that file.";
        session.ActiveEditor = editor;
        return $"Switched active editor to '{filepath}'.";
    }

    /// <summary>
    /// Exits the editor.
    /// </summary>
    /// <returns>A message indicating the result of the operation.</returns>
    private string ExitCommand()
    {
        // On exit, ask if unsaved files should be saved
        // foreach (var editor in session.Editors)
        // {
        //     if (editor.IsDirty)
        //     {
        //         Console.Write($"File '{editor.FilePath}' not saved, save? (y/n): ");
        //         var ans = Console.ReadLine();
        //         if (ans?.ToLower() == "y")
        //         {
        //             editor.Save();
        //         }
        //     }
        // }
        return "Exiting editor.";
    }

    /// <summary>
    /// Performs a spell check on the active editor.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The result is a spell check report.</returns>
    private async Task<string> PerformSpellCheckOnActiveEditor()
    {
        ValidateActiveEditor();
        var report = await session.ActiveEditor.HtmlEditor.PerformSpellCheckAsync();
        return report;
    }

    /// <summary>
    /// Prints the HTML tree structure of the active editor.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The result is a string containing the HTML tree structure and spell check report.</returns>
    private async Task<string> PrintTreeCommand()
    {
        ValidateActiveEditor();

        // Perform spell check and get the report
        var spellResult = await PerformSpellCheckOnActiveEditor();

        // Get the HTML tree structure
        var htmlTree = session.ActiveEditor.HtmlEditor.RootHtml.PrintTree(session.ActiveEditor.ShowId, markError: true);

        // Return the spell check report and tree structure
        return $"{spellResult}\nHTML Tree:\n{htmlTree}";
    }

    /// <summary>
    /// Prints the indented HTML of the active editor.
    /// </summary>
    /// <param name="arg">The indentation size (optional).</param>
    /// <returns>A task that represents the asynchronous operation. The result is a string containing the indented HTML and spell check report.</returns>
    private async Task<string> PrintIndentCommand(string arg)
    {
        ValidateActiveEditor();

        // Perform spell check and get the report
        var spellResult = await PerformSpellCheckOnActiveEditor();

        // Determine the indentation size
        int indentSize = 4; // Default indentation width
        if (!string.IsNullOrWhiteSpace(arg) && int.TryParse(arg, out var val))
        {
            indentSize = val;
        }

        // Get the indented HTML string
        var htmlIndented = session.ActiveEditor.HtmlEditor.PrintIndent(indentSize);

        // Return the spell check report and indented structure
        return $"{spellResult}\nIndented HTML:\n{htmlIndented}";
    }

    /// <summary>
    /// Toggles the display of IDs in the HTML tree.
    /// </summary>
    /// <param name="arg">The toggle value ("true" or "false").</param>
    /// <returns>A message indicating the result of the operation.</returns>
    private string ShowIdCommand(string arg)
    {
        ValidateActiveEditor();
        if (arg.ToLower() == "true")
        {
            session.ActiveEditor.ShowId = true;
            return "ShowId enabled.";
        }
        else if (arg.ToLower() == "false")
        {
            session.ActiveEditor.ShowId = false;
            return "ShowId disabled.";
        }
        return "Error: invalid argument, use showid true/false.";
    }

    /// <summary>
    /// Prints the directory tree.
    /// </summary>
    /// <returns>A string representing the directory tree.</returns>
    private string DirTreeCommand()
    {
        string currentDir = Directory.GetCurrentDirectory();
        return DirPrinter.PrintDirTree(currentDir, session.Editors);
    }

    /// <summary>
    /// Prints the directory with indentation.
    /// </summary>
    /// <param name="arg">The indentation size (optional).</param>
    /// <returns>A string representing the indented directory.</returns>
    private string DirIndentCommand(string arg)
    {
        string currentDir = Directory.GetCurrentDirectory();
        int indent = 4; // Default indentation is 4
        if (!string.IsNullOrWhiteSpace(arg) && int.TryParse(arg, out var val))
        {
            indent = val;
        }
        return DirPrinter.PrintDirIndent(currentDir, session.Editors, indent);
    }

    /// <summary>
    /// Validates that there is an active editor.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when there is no active editor.</exception>
    private void ValidateActiveEditor()
    {
        if (session.ActiveEditor == null) throw new InvalidOperationException("No active editor.");
    }

    /// <summary>
    /// Splits the arguments string into an array of arguments.
    /// </summary>
    /// <param name="arguments">The arguments string.</param>
    /// <returns>An array of arguments.</returns>
    private string[] SplitArguments(string arguments)
    {
        var matches = Regex.Matches(arguments, @"[\""].+?[\""]|[^ ]+")
            .Cast<Match>()
            .Select(m => m.Value.Trim('"'))
            .ToArray();
        return matches;
    }
}