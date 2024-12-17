using System;
using System.IO;
using System.Text.Json.Serialization;

/// <summary>
/// Represents an editor for managing HTML files, with features for saving and tracking changes.
/// </summary>
public class Editor
{
    /// <summary>
    /// The HTML editor instance used for managing the content of the associated file.
    /// This property is ignored during JSON serialization.
    /// </summary>
    [JsonIgnore]
    public HtmlEditor HtmlEditor { get; private set; } = new HtmlEditor();

    /// <summary>
    /// Indicates whether the file has unsaved changes.
    /// </summary>
    public bool IsDirty { get; set; }

    /// <summary>
    /// Determines whether to display the file ID when rendering its representation.
    /// Defaults to true.
    /// </summary>
    public bool ShowId { get; set; } = true;

    /// <summary>
    /// The file path associated with the current editor instance.
    /// </summary>
    public string FilePath { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Editor"/> class with the specified file path.
    /// </summary>
    /// <param name="filePath">The file path to be managed by this editor.</param>
    public Editor(string filePath)
    {
        FilePath = filePath;
        if (FileHandler.FileExists(filePath))
        {
            HtmlEditor.Read(filePath);
            IsDirty = false;
        }
        else
        {
            HtmlEditor.Init();
            Save();
            IsDirty = true;
        }
    }

    /// <summary>
    /// Saves the current content of the editor to the associated file path.
    /// </summary>
    public void Save()
    {
        HtmlEditor.Save(FilePath);
        IsDirty = false;
    }

    /// <summary>
    /// Placeholder method for closing the editor.
    /// The actual implementation is expected to be handled in a session context.
    /// </summary>
    public void Close()
    {
        // Close operation should be implemented in the session context. This serves as an interface.
    }
}
