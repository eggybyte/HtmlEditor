using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    /// <summary>
    /// The path to the temporary folder used for storing session data.
    /// </summary>
    private const string TempFolder = ".temp";

    /// <summary>
    /// The main entry point for the HTML Editor application.
    /// This method initializes the session state, handles user commands,
    /// and performs the necessary actions until the user decides to exit.
    /// </summary>
    /// <returns>An asynchronous task representing the operation.</returns>
    static async Task Main()
    {
        // Load the session state from the previous session (if any).
        var session = SessionStateManager.LoadSessionState(TempFolder);

        // Initialize the command handler to process user input.
        var commandHandler = new CommandHandler(session);
        Console.WriteLine("Welcome to the HTML Editor. Type 'help' to see available commands.");

        // Start a loop to continuously read user input and execute commands.
        while (true)
        {
            // Display a prompt and wait for user input.
            Console.Write("> ");
            string? input = Console.ReadLine();
            if (string.IsNullOrEmpty(input)) continue; // Skip empty inputs.

            // Execute the user command asynchronously and get the result.
            var result = await commandHandler.ExecuteCommandAsync(input);
            Console.WriteLine(result);

            // Exit the loop if the user types "exit".
            if (input.StartsWith("exit", StringComparison.OrdinalIgnoreCase))
            {
                // Before exiting, save the session state to preserve the current session.
                SessionStateManager.SaveSessionState(session, TempFolder);
                break;
            }
        }
    }
}
