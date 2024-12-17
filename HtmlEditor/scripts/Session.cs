using System.Collections.Generic;

/// <summary>
/// Represents a session that holds a collection of editors and tracks the active editor.
/// </summary>
/// <remarks>
/// A session is typically used in a multi-editor context, where multiple editors are present, 
/// and one editor is active at any given time. This class provides methods to add, remove, 
/// and retrieve editors, as well as to track the session's state.
/// </remarks>
public class Session
{
    /// <summary>
    /// Gets the list of editors associated with the session.
    /// </summary>
    /// <value>
    /// A list containing the editors in the session.
    /// </value>
    public List<Editor> Editors { get; private set; } = new List<Editor>();

    /// <summary>
    /// Gets or sets the currently active editor in the session.
    /// </summary>
    /// <value>
    /// The currently active editor or null if no editor is active.
    /// </value>
    public Editor ActiveEditor { get; set; }

    /// <summary>
    /// Retrieves an editor from the session by its file path.
    /// </summary>
    /// <param name="filePath">The file path of the editor to retrieve.</param>
    /// <returns>
    /// The editor associated with the given file path, or null if no editor matches the file path.
    /// </returns>
    public Editor GetEditorByFilePath(string filePath)
    {
        return Editors.Find(e => e.FilePath == filePath);
    }

    /// <summary>
    /// Adds a new editor to the session and sets it as the active editor.
    /// </summary>
    /// <param name="editor">The editor to add to the session.</param>
    public void AddEditor(Editor editor)
    {
        Editors.Add(editor);
        ActiveEditor = editor;
    }

    /// <summary>
    /// Removes an editor from the session. If the removed editor is the active editor,
    /// the next editor in the list becomes the active editor.
    /// </summary>
    /// <param name="editor">The editor to remove from the session.</param>
    public void RemoveEditor(Editor editor)
    {
        Editors.Remove(editor);
        if (ActiveEditor == editor)
        {
            ActiveEditor = Editors.Count > 0 ? Editors[0] : null;
        }
    }

    /// <summary>
    /// Gets the state of all editors in the session, including their file path and ID visibility.
    /// </summary>
    /// <returns>
    /// An array of <see cref="EditorState"/> representing the state of all editors.
    /// </returns>
    public EditorState[] GetEditorsState()
    {
        var list = new List<EditorState>();
        foreach (var e in Editors)
        {
            list.Add(new EditorState
            {
                FilePath = e.FilePath,
                ShowId = e.ShowId
            });
        }
        return list.ToArray();
    }
}
