using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

public class HtmlEditorCommandTests
{
    /// <summary>
    /// Command handler for processing user commands.
    /// </summary>
    private readonly CommandHandler commandHandler;

    /// <summary>
    /// Current session for maintaining state.
    /// </summary>
    private readonly Session session;

    /// <summary>
    /// Path to the temporary folder used for storing session data.
    /// </summary>
    private const string TempFolder = ".temp";

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlEditorCommandTests"/> class.
    /// Loads the session state from the specified temporary folder and initializes the command handler.
    /// </summary>
    public HtmlEditorCommandTests()
    {
        // Load the session state from the temporary folder (if any).
        session = SessionStateManager.LoadSessionState(TempFolder);
        // Initialize the command handler with the loaded session.
        commandHandler = new CommandHandler(session);
    }

    /// <summary>
    /// Test method that executes commands from a file sequentially and logs the results.
    /// The method reads a list of commands from a text file, processes them through the command handler, 
    /// and records the output to a log file.
    /// </summary>
    /// <returns>An asynchronous task representing the operation.</returns>
    [Fact]
    public async Task TestCommandSequenceFromFileAsync()
    {
        // Define the path to the command file.
        string filePath = "commands.txt";

        // Check if the command file exists.
        if (!FileHandler.FileExists(filePath))
        {
            // If the file doesn't exist, throw an exception.
            throw new FileNotFoundException($"Command file not found: {filePath}");
        }

        // Read the commands from the file into a list of strings.
        var commands = FileHandler.ReadAllLines(filePath);
        // Create a list to store the log entries for each command execution.
        List<string> logEntries = new List<string>();

        // Iterate through each command in the file.
        foreach (var commandLine in commands)
        {
            // Log the command being executed.
            logEntries.Add($"Command: {commandLine}");
            // Execute the command asynchronously and capture the result.
            string result = await commandHandler.ExecuteCommandAsync(commandLine);
            // Log the output result.
            logEntries.Add($"Output:\n{result}\n");
            // Optionally, print the result to the console.
            Console.WriteLine(result);
        }

        // Write the log entries to an output log file.
        FileHandler.WriteAllLines("command_output_log.txt", [.. logEntries]);

        // Save the final session state after executing all commands.
        SessionStateManager.SaveSessionState(session, TempFolder);
    }
}
