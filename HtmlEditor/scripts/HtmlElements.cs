/// <summary>
/// Specialized HTML elements for the document, including Html, Head, Body, and Title elements.
/// </summary>
/// <remarks>
/// Contains specific elements for typical HTML structure such as head and body.
/// </remarks>
public class Html : HTMLElement
{
    /// <summary>
    /// Gets the Head element of the HTML document.
    /// </summary>
    public Head Head { get; private set; }

    /// <summary>
    /// Gets the Body element of the HTML document.
    /// </summary>
    public Body Body { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Html"/> class.
    /// </summary>
    public Html() : base("html", "html")
    {
        Head = new Head();
        Body = new Body();
        AddChild(Head);
        AddChild(Body);
    }

    /// <summary>
    /// Clones the Html element and its children.
    /// </summary>
    /// <returns>A cloned instance of the Html element.</returns>
    public override HTMLElement Clone()
    {
        var clonedHtml = new Html();

        // Clone Head and Body as new instances
        clonedHtml.Head = (Head)Head.Clone();
        clonedHtml.Body = (Body)Body.Clone();

        clonedHtml.Children.Clear(); // Clear existing children
        clonedHtml.AddChild(clonedHtml.Head); // Add cloned Head and Body
        clonedHtml.AddChild(clonedHtml.Body);

        return clonedHtml;
    }
}

/// <summary>
/// Represents the Head element of an HTML document.
/// </summary>
public class Head : HTMLElement
{
    /// <summary>
    /// Gets the Title element of the Head.
    /// </summary>
    public Title Title { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Head"/> class.
    /// </summary>
#nullable disable
    public Head() : base("head", "head")
    {
        SetTitle("");
    }
#nullable restore

    /// <summary>
    /// Sets the content of the Title element.
    /// </summary>
    /// <param name="content">The content text for the Title element.</param>
    public void SetTitle(string content)
    {
        if (Title == null)
            Title = new Title("title", content);
        else
            Title.Content = content;

        if (!Children.Contains(Title))
            AddChild(Title);
    }

    /// <summary>
    /// Clones the Head element and its children.
    /// </summary>
    /// <returns>A cloned instance of the Head element.</returns>
    public override HTMLElement Clone()
    {
        var clonedHead = new Head();
        if (Title != null)
        {
            clonedHead.SetTitle(Title.Content);
        }
        foreach (HTMLElement child in Children)
        {
            if (child != Title)
            {
                clonedHead.AddChild(child.Clone());
            }
        }
        return clonedHead;
    }
}

/// <summary>
/// Represents the Body element of an HTML document.
/// </summary>
public class Body : HTMLElement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Body"/> class.
    /// </summary>
    public Body() : base("body", "body") { }

    /// <summary>
    /// Clones the Body element and its children.
    /// </summary>
    /// <returns>A cloned instance of the Body element.</returns>
    public override HTMLElement Clone()
    {
        var clonedBody = new Body();
        foreach (var child in Children)
        {
            clonedBody.AddChild(child.Clone());
        }
        return clonedBody;
    }
}

/// <summary>
/// Represents the Title element of an HTML document.
/// </summary>
public class Title : HTMLElement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Title"/> class.
    /// </summary>
    /// <param name="id">The unique ID of the Title element.</param>
    /// <param name="content">The content text for the Title element.</param>
    public Title(string id, string content) : base("title", id, content) { }
}

/// <summary>
/// Represents a Paragraph element in an HTML document.
/// </summary>
public class Paragraph : HTMLElement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Paragraph"/> class.
    /// </summary>
    /// <param name="id">The unique ID of the Paragraph element.</param>
    /// <param name="content">The content text for the Paragraph element.</param>
    public Paragraph(string id, string content) : base("p", id, content) { }
}

/// <summary>
/// Represents a Div element in an HTML document.
/// </summary>
public class Div : HTMLElement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Div"/> class.
    /// </summary>
    /// <param name="id">The unique ID of the Div element.</param>
    /// <param name="content">The content text for the Div element.</param>
    public Div(string id, string content) : base("div", id, content) { }
}

/// <summary>
/// Represents a Header element in an HTML document.
/// </summary>
public class Header : HTMLElement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Header"/> class.
    /// </summary>
    /// <param name="id">The unique ID of the Header element.</param>
    /// <param name="content">The content text for the Header element.</param>
    /// <param name="level">The header level (1-6).</param>
    public Header(string id, string content, int level) : base($"h{level}", id, content) { }
}

/// <summary>
/// Represents a generic HTML element for non-specific tags.
/// </summary>
public class GenericElement : HTMLElement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenericElement"/> class.
    /// </summary>
    /// <param name="tagName">The HTML tag name of the element.</param>
    /// <param name="id">The unique ID of the element.</param>
    /// <param name="content">The content text for the element (optional).</param>
    public GenericElement(string tagName, string id, string content = "") : base(tagName, id, content) { }

    /// <summary>
    /// Clones the GenericElement and its children.
    /// </summary>
    /// <returns>A cloned instance of the GenericElement.</returns>
    public override HTMLElement Clone()
    {
        var clonedElement = new GenericElement(TagName, Id, Content);
        foreach (var child in Children)
        {
            clonedElement.AddChild(child.Clone());
        }
        return clonedElement;
    }
}
