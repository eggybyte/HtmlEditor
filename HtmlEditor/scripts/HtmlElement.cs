using System.Text;

/// <summary>
/// Abstract base class for HTML elements, implementing the ITreeNode interface.
/// </summary>
public abstract class HTMLElement : ITreeNode
{
    /// <summary>
    /// Gets the tag name of the HTML element.
    /// </summary>
    public string TagName { get; }

    /// <summary>
    /// Gets or sets the ID of the HTML element.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Gets or sets the content of the HTML element.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Gets the list of child elements for this HTML element.
    /// </summary>
    public List<HTMLElement> Children { get; private set; } = new List<HTMLElement>();

    /// <summary>
    /// Gets or sets the parent element of this HTML element.
    /// </summary>
    public HTMLElement Parent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the element has a spelling error.
    /// </summary>
    public bool IsSpellError { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HTMLElement"/> class with a tag name, ID, and optional content.
    /// </summary>
    /// <param name="tagName">The tag name of the HTML element.</param>
    /// <param name="id">The ID of the HTML element.</param>
    /// <param name="content">The content of the HTML element (optional).</param>
    protected HTMLElement(string tagName, string id, string content = "")
    {
        TagName = tagName;
        Id = id;
        Content = content;
    }

    /// <summary>
    /// Sets a new ID for the HTML element.
    /// </summary>
    /// <param name="newId">The new ID to assign to the element.</param>
    public void SetId(string newId)
    {
        Id = newId;
    }

    /// <summary>
    /// Adds a child element to this HTML element.
    /// </summary>
    /// <param name="child">The child element to add.</param>
    public void AddChild(HTMLElement child)
    {
        if (!Children.Contains(child))
        {
            child.Parent = this;
            Children.Add(child);
        }
    }

    /// <summary>
    /// Removes a child element from this HTML element.
    /// </summary>
    /// <param name="child">The child element to remove.</param>
    public void RemoveChild(HTMLElement child)
    {
        Children.Remove(child);
    }

    /// <summary>
    /// Finds an element by its ID, starting from this element.
    /// </summary>
    /// <param name="id">The ID of the element to search for.</param>
    /// <returns>The HTMLElement with the specified ID, or null if no element with that ID is found.</returns>
    public HTMLElement FindById(string id)
    {
        if (Id == id) return this;
        foreach (var child in Children)
        {
            var result = child.FindById(id);
            if (result != null) return result;
        }
        return null;
    }

    /// <summary>
    /// Clones the current element and its children.
    /// </summary>
    /// <returns>A new instance of <see cref="HTMLElement"/> that is a clone of the current element.</returns>
    public virtual HTMLElement Clone()
    {
        var cloned = (HTMLElement)Activator.CreateInstance(GetType(), Id, Content);
        foreach (var c in Children)
        {
            cloned.AddChild(c.Clone());
        }
        return cloned;
    }

    /// <summary>
    /// Renders the HTML element and its children to a string, with optional indentation.
    /// </summary>
    /// <param name="indentLevel">The level of indentation (default is 0).</param>
    /// <param name="indentSize">The number of spaces for each indentation level (default is 2).</param>
    /// <returns>A string representation of the HTML element.</returns>
    public virtual string Render(int indentLevel = 0, int indentSize = 2)
    {
        string indent = new string(' ', indentLevel * indentSize);
        var sb = new StringBuilder();
        sb.Append($"{indent}<{TagName}");
        if (!IsSpecialTag() && !string.IsNullOrEmpty(Id))
        {
            sb.Append($" id=\"{Id}\"");
        }
        sb.Append(">");
        if (!string.IsNullOrEmpty(Content))
            sb.Append(Content);

        if (Children.Count > 0)
        {
            sb.AppendLine();
            foreach (var child in Children)
            {
                sb.Append(child.Render(indentLevel + 1, indentSize));
                sb.AppendLine();
            }
            sb.Append($"{indent}</{TagName}>");
        }
        else
        {
            sb.Append($"</{TagName}>");
        }
        return sb.ToString();
    }

    /// <summary>
    /// Prints the tree structure of the HTML element and its children.
    /// </summary>
    /// <param name="showId">Whether to display the element's ID (default is true).</param>
    /// <param name="markError">Whether to mark elements with spelling errors (default is true).</param>
    /// <returns>A string representation of the tree structure of the element.</returns>
    public string PrintTree(bool showId = true, bool markError = true)
    {
        return TreePrinter.Print(this, showId, markError: markError);
    }

    // Implement ITreeNode interface
    IEnumerable<ITreeNode> ITreeNode.Children => Children.Cast<ITreeNode>();

    /// <summary>
    /// Gets the output content of the element, based on whether to show the ID and whether the tag is special.
    /// </summary>
    /// <param name="showId">Whether to show the ID of the element.</param>
    /// <returns>A string representing the element's output content.</returns>
    public virtual string GetOutputContent(bool showId)
    {
        if (IsSpecialTag())
        {
            return TagName;
        }

        return showId && !string.IsNullOrEmpty(Id) ? $"{TagName}#{Id}" : TagName;
    }

    /// <summary>
    /// Determines whether the tag is a special HTML tag (such as html, head, title, or body).
    /// </summary>
    /// <returns>True if the tag is special (html, head, title, or body); otherwise, false.</returns>
    protected bool IsSpecialTag()
    {
        return TagName == "html" || TagName == "head" || TagName == "title" || TagName == "body";
    }
}
