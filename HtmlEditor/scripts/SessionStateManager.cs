using System;
using System.IO;
using System.Text.Json;

/// <summary>
/// A static class that manages the loading and saving of session state.
/// This class is responsible for saving the state of the current session (including the editors) 
/// and loading it back when needed.
/// </summary>
public static class SessionStateManager
{
    /// <summary>
    /// The name of the state file where session data is stored.
    /// </summary>
    private const string StateFileName = "session_state.json";

    /// <summary>
    /// Loads the session state from a JSON file in the specified folder.
    /// If the state file does not exist or is corrupted, a new empty session is returned.
    /// </summary>
    /// <param name="tempFolder">The folder where the session state file is stored.</param>
    /// <returns>
    /// A <see cref="Session"/> object representing the loaded session state.
    /// If the file does not exist or fails to load, an empty session is returned.
    /// </returns>
    public static Session LoadSessionState(string tempFolder)
    {
        // Generate the full path to the state file
        string stateFilePath = Path.Combine(tempFolder, StateFileName);

        // Check if the state file exists
        if (!FileHandler.FileExists(stateFilePath))
        {
            // Return a new empty session if the file doesn't exist
            return new Session();
        }

        try
        {
            // Read the content of the state file
            string json = FileHandler.ReadAllText(stateFilePath);
            // Deserialize the JSON into a SessionState object
            var state = JsonSerializer.Deserialize<SessionState>(json);

            // Create a new session
            var session = new Session();

            // Iterate over each editor state and reconstruct the editors
            foreach (var editorState in state.Editors)
            {
                // Create a new editor based on the stored file path
                var editor = new Editor(editorState.FilePath);
                editor.ShowId = editorState.ShowId;

                // Check if the file exists
                if (FileHandler.FileExists(editor.FilePath))
                {
                    Console.WriteLine($"Reading file: {editor.FilePath}");
                    editor.HtmlEditor.Read(editor.FilePath);  // Read the content if the file exists
                }
                else
                {
                    Console.WriteLine($"File not found: {editor.FilePath}. Initializing empty document.");
                    editor.HtmlEditor.Init();  // Initialize a new empty document if the file doesn't exist
                    editor.IsDirty = true;  // Mark the editor as dirty (unsaved)
                }

                // Add the editor to the session
                session.AddEditor(editor);
            }

            // Set the active editor if specified in the state
            if (!string.IsNullOrEmpty(state.ActiveEditorFilePath))
            {
                var activeEditor = session.GetEditorByFilePath(state.ActiveEditorFilePath);
                if (activeEditor != null)
                {
                    session.ActiveEditor = activeEditor;
                }
            }

            // Return the session object
            return session;
        }
        catch
        {
            // If the state file is corrupted or an error occurs, return a new empty session
            return new Session();
        }
    }

    /// <summary>
    /// Saves the current session state to a JSON file in the specified folder.
    /// The session state includes the active editor and the states of all editors.
    /// </summary>
    /// <param name="session">The session object containing the current state to save.</param>
    /// <param name="tempFolder">The folder where the session state file should be saved.</param>
    public static void SaveSessionState(Session session, string tempFolder)
    {
        // Generate the full path to the state file
        string stateFilePath = Path.Combine(tempFolder, StateFileName);

        // Create a SessionState object from the current session's data
        var state = new SessionState
        {
            ActiveEditorFilePath = session.ActiveEditor?.FilePath,  // Store the active editor's file path
            Editors = session.GetEditorsState()  // Store the states of all editors
        };

        // Serialize the state object to JSON with indentation for readability
        string json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });

        // Write the serialized JSON to the state file
        FileHandler.WriteAllText(stateFilePath, json);
    }
}

/// <summary>
/// Represents the state of a session, including the active editor and all editors in the session.
/// </summary>
public class SessionState
{
    /// <summary>
    /// Gets or sets the file path of the active editor in the session.
    /// </summary>
    public string ActiveEditorFilePath { get; set; }

    /// <summary>
    /// Gets or sets the states of all editors in the session.
    /// </summary>
    public EditorState[] Editors { get; set; }
}

/// <summary>
/// Represents the state of an individual editor, including its file path and whether the ID should be shown.
/// </summary>
public class EditorState
{
    /// <summary>
    /// Gets or sets the file path of the editor.
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the ID of the editor should be shown.
    /// </summary>
    public bool ShowId { get; set; }
}
