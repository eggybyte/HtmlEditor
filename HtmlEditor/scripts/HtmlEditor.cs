// HtmlEditor.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#nullable disable

/// <summary>
/// Manages HTML document editing, saving, loading, and printing functionalities.
/// </summary>
public class HtmlEditor
{
    /// <summary>
    /// Gets or sets the root HTML element.
    /// </summary>
    public Html RootHtml { get; set; }

    /// <summary>
    /// Manages undo and redo history of HTML state changes.
    /// </summary>
    private HistoryManager historyManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlEditor"/> class.
    /// </summary>
    public HtmlEditor()
    {
        RootHtml = new Html();
        historyManager = new HistoryManager();
    }

    /// <summary>
    /// Initializes the editor by creating a new empty HTML document and saving the initial state.
    /// </summary>
    public void Init()
    {
        RootHtml = new Html();
        historyManager.SaveState(RootHtml);
    }

    /// <summary>
    /// Reads an HTML file and parses its content into the editor.
    /// </summary>
    /// <param name="filepath">The path to the HTML file.</param>
    /// <exception cref="FileNotFoundException">Thrown when the file is not found.</exception>
    public void Read(string filepath)
    {
        string content = "";

        if (FileHandler.FileExists(filepath))
        {
            content = FileHandler.ReadAllText(filepath);
        }
        else
        {
            // Use initialization template if the file does not exist
            RootHtml = new Html();
            historyManager.SaveState(RootHtml);
            return;
        }

        ParseHtml(content);
        historyManager.SaveState(RootHtml);
    }

    /// <summary>
    /// Saves the current HTML content to the specified file.
    /// </summary>
    /// <param name="filepath">The path to save the file.</param>
    public void Save(string filepath)
    {
        FileHandler.WriteAllText(filepath, RootHtml.Render());
    }

    /// <summary>
    /// Inserts a new HTML element at the specified location.
    /// </summary>
    /// <param name="tagName">The tag name of the new element.</param>
    /// <param name="id">The ID of the new element.</param>
    /// <param name="insertLocation">The ID of the element before which the new element will be inserted.</param>
    /// <param name="textContent">The text content of the new element (optional).</param>
    /// <exception cref="ArgumentException">Thrown when the insertion location element is not found.</exception>
    public void Insert(string tagName, string id, string insertLocation, string textContent = "")
    {
        var locationElement = RootHtml.FindById(insertLocation)
            ?? throw new ArgumentException("Location element not found.");

        var newElement = TagFactory.CreateElement(tagName, id, textContent);
        newElement.Parent = locationElement.Parent;

        int index = locationElement.Parent.Children.IndexOf(locationElement);
        locationElement.Parent.Children.Insert(index, newElement);

        historyManager.SaveState(RootHtml);
    }

    /// <summary>
    /// Appends a new HTML element to the specified parent element.
    /// </summary>
    /// <param name="tagName">The tag name of the new element.</param>
    /// <param name="id">The ID of the new element.</param>
    /// <param name="parentId">The ID of the parent element.</param>
    /// <param name="textContent">The text content of the new element (optional).</param>
    /// <exception cref="ArgumentException">Thrown when the parent element is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when an element with the same ID already exists.</exception>
    public void Append(string tagName, string id, string parentId, string textContent = "")
    {
        var parentElement = RootHtml.FindById(parentId)
            ?? throw new ArgumentException("Parent element not found.");

        var newElement = TagFactory.CreateElement(tagName, id, textContent);
        if (RootHtml.FindById(id) != null)
        {
            throw new InvalidOperationException("Element with ID " + id + " already exists in the tree.");
        }
        parentElement.AddChild(newElement);

        historyManager.SaveState(RootHtml);
    }

    /// <summary>
    /// Edits the ID of an existing element.
    /// </summary>
    /// <param name="oldId">The current ID of the element.</param>
    /// <param name="newId">The new ID for the element.</param>
    /// <exception cref="ArgumentException">Thrown when the element is not found or the new ID already exists.</exception>
    public void EditId(string oldId, string newId)
    {
        var element = RootHtml.FindById(oldId);
        if (element == null) throw new ArgumentException("Element not found.");

        if (RootHtml.FindById(newId) != null)
        {
            throw new ArgumentException($"{newId} already exists.");
        }

        element.SetId(newId);
        historyManager.SaveState(RootHtml);
    }

    /// <summary>
    /// Edits the text content of an existing element.
    /// </summary>
    /// <param name="elementId">The ID of the element to edit.</param>
    /// <param name="newTextContent">The new text content for the element.</param>
    /// <exception cref="ArgumentException">Thrown when the element is not found.</exception>
    public void EditText(string elementId, string newTextContent)
    {
        var element = RootHtml.FindById(elementId);
        if (element == null) throw new ArgumentException("Element not found.");

        element.Content = newTextContent;
        historyManager.SaveState(RootHtml);
    }

    /// <summary>
    /// Deletes an existing HTML element.
    /// </summary>
    /// <param name="elementId">The ID of the element to delete.</param>
    /// <exception cref="ArgumentException">Thrown when the element or its parent is not found.</exception>
    public void Delete(string elementId)
    {
        var element = RootHtml.FindById(elementId);
        if (element == null || element.Parent == null)
            throw new ArgumentException("Element or parent not found.");

        element.Parent.RemoveChild(element);
        historyManager.SaveState(RootHtml);
    }

    /// <summary>
    /// Prints the HTML tree structure, optionally showing IDs and marking spelling errors.
    /// </summary>
    /// <param name="showId">Indicates whether to display element IDs.</param>
    /// <param name="markError">Indicates whether to mark elements with spelling errors.</param>
    /// <returns>A formatted string representing the tree structure.</returns>
    public string PrintTree(bool showId = true, bool markError = false)
    {
        return RootHtml.PrintTree(showId, markError);
    }

    /// <summary>
    /// Converts the current HTML tree structure to an indented string representation.
    /// </summary>
    /// <param name="indentSize">The number of spaces per indentation level (default is 4).</param>
    /// <returns>An indented string representation of the HTML tree structure.</returns>
    public string PrintIndent(int indentSize = 4)
    {
        return RootHtml.Render(0, indentSize);
    }

    /// <summary>
    /// Performs an undo operation, reverting to the previous state in the history.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when there are no undo operations available.</exception>
    public void Undo()
    {
        RootHtml = historyManager.Undo();
    }

    /// <summary>
    /// Performs a redo operation, reverting to the last undone state.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when there are no redo operations available.</exception>
    public void Redo()
    {
        RootHtml = historyManager.Redo();
    }

    /// <summary>
    /// Parses HTML content and constructs the internal HTML tree structure.
    /// </summary>
    /// <param name="content">The HTML content to parse.</param>
    private void ParseHtml(string content)
    {
        // Initialize the root <html> element, including default <head> and <body> structures
        RootHtml = new Html();
        Head headElement = RootHtml.Head;
        Body bodyElement = RootHtml.Body;

        var lines = content.Split('\n');
        Stack<HTMLElement> elementStack = new Stack<HTMLElement>();
        elementStack.Push(RootHtml);

        foreach (var line in lines)
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine)) continue;

            if (trimmedLine.StartsWith("<") && trimmedLine.Contains(">"))
            {
                bool isClosingTag = trimmedLine.StartsWith("</");
                bool isSelfClosing = trimmedLine.EndsWith("/>");

                // Extract the tag name (the first part of the tag)
                string tagName = trimmedLine.Split(new[] { ' ', '>', '/' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim('<', '>', '/');
                if (tagName.Equals("html", StringComparison.OrdinalIgnoreCase)) continue;

                if (isClosingTag)
                {
                    // If it's a closing tag, pop the current element from the stack
                    if (elementStack.Count > 1)
                    {
                        elementStack.Pop();
                    }
                    continue;
                }

                if (tagName.Equals("title", StringComparison.OrdinalIgnoreCase))
                {
                    string titleContent = ExtractContent(trimmedLine);
                    headElement.SetTitle(titleContent);
                    continue;
                }

                HTMLElement element;
                if (tagName.Equals("head", StringComparison.OrdinalIgnoreCase) || tagName.Equals("body", StringComparison.OrdinalIgnoreCase))
                {
                    // Skip adding <head> and <body>, as they already exist in RootHtml
                    element = tagName.Equals("head", StringComparison.OrdinalIgnoreCase) ? headElement : bodyElement;
                }
                else
                {
                    // General element, with optional ID or content
                    string id = ExtractAttribute(trimmedLine, "id");
                    string elementContent = ExtractContent(trimmedLine);

                    // If the tag is <ul>, <ol>, etc., treat it as a container for <li> elements
                    if (tagName.Equals("ul", StringComparison.OrdinalIgnoreCase) || tagName.Equals("ol", StringComparison.OrdinalIgnoreCase))
                    {
                        element = new GenericElement(tagName, id, elementContent);
                    }
                    else
                    {
                        // Use TagFactory to create the element
                        element = TagFactory.CreateElement(tagName, id, elementContent);
                    }
                }

                // Add the created element as a child of the current top element in the stack
                elementStack.Peek().AddChild(element);

                // If it's not a self-closing tag, push the element onto the stack
                if (!isSelfClosing && !(element is GenericElement && (element.TagName.Equals("ul", StringComparison.OrdinalIgnoreCase) || element.TagName.Equals("ol", StringComparison.OrdinalIgnoreCase))))
                {
                    elementStack.Push(element);
                }
            }
            else
            {
                // Handle text content, can be extended based on requirements
            }
        }
    }

    /// <summary>
    /// Extracts the value of a specified attribute from an HTML tag line.
    /// </summary>
    /// <param name="line">The HTML tag line.</param>
    /// <param name="attribute">The name of the attribute to extract.</param>
    /// <returns>The attribute value, or an empty string if not found.</returns>
    private string ExtractAttribute(string line, string attribute)
    {
        var pattern = $"{attribute}=\"([^\"]+)\"";
        var match = Regex.Match(line, pattern);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    /// <summary>
    /// Extracts the text content from an HTML tag line.
    /// </summary>
    /// <param name="line">The HTML tag line.</param>
    /// <returns>The text content of the element, or an empty string if not found.</returns>
    private string ExtractContent(string line)
    {
        // If the tag is self-closing (e.g., <img />), do not extract content
        if (line.EndsWith("/>")) return string.Empty;

        int startIndex = line.IndexOf('>') + 1;
        int endIndex = line.LastIndexOf("</");
        if (startIndex >= 0 && endIndex > startIndex)
        {
            return line.Substring(startIndex, endIndex - startIndex).Trim();
        }
        return string.Empty;
    }

    /// <summary>
    /// Performs spell checking asynchronously and marks elements with spelling errors.
    /// </summary>
    /// <returns>A string report of spelling errors or a success message.</returns>
    /// <exception cref="HttpRequestException">Thrown on API call errors.</exception>
    /// <remarks>
    /// Provides asynchronous spell checking using the LanguageTool API.
    /// Supports recursive checking and error context within HTML elements.
    /// </remarks>
    public async Task<string> PerformSpellCheckAsync()
    {
        // Perform spell checking and get the report
        string report = await SpellChecker.CheckSpellingAsync(RootHtml);

        // Save the state after spell checking
        historyManager.SaveState(RootHtml);

        return report;
    }

    /// <summary>
    /// Traverses all HTML elements starting from the specified root.
    /// </summary>
    /// <param name="root">The root element.</param>
    /// <returns>An enumerable collection of all traversed elements.</returns>
    private IEnumerable<HTMLElement> TraverseElements(HTMLElement root)
    {
        yield return root;
        foreach (var child in root.Children)
        {
            foreach (var descendant in TraverseElements(child))
            {
                yield return descendant;
            }
        }
    }
}