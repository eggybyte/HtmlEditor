/// <summary>
/// Factory class for creating HTML elements based on tag name.
/// </summary>
public static class TagFactory
{
    /// <summary>
    /// Creates a new HTML element based on the specified tag name.
    /// </summary>
    /// <param name="tagName">The HTML tag name (e.g., "p", "div", "h1").</param>
    /// <param name="id">The identifier for the element.</param>
    /// <param name="content">Optional text content for the element.</param>
    /// <returns>A new instance of <see cref="HTMLElement"/> based on the tag name.</returns>
    public static HTMLElement CreateElement(string tagName, string id, string content = "")
    {
        try
        {
            return tagName.ToLower() switch
            {
                "div" => new Div(id, content),
                "p" => new Paragraph(id, content),
                "h1" => new Header(id, content, 1),
                "h2" => new Header(id, content, 2),
                "h3" => new Header(id, content, 3),
                "title" => new Title(id, content),
                _ => new GenericElement(tagName, id, content),
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating element '{tagName}': {ex.Message}");
            throw;
        }
    }
}