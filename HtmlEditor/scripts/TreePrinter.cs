using System.Text;

public static class TreePrinter
{
    /// <summary>
    /// Prints the tree structure of a given root node.
    /// </summary>
    /// <param name="root">The root node of the tree.</param>
    /// <param name="showId">Specifies whether to display the ID of each node. Default is true.</param>
    /// <param name="markError">Specifies whether to mark nodes with spelling errors. Default is false.</param>
    /// <param name="useSymbols">Specifies whether to use tree symbols (└── and ├──) for hierarchy representation. Default is true.</param>
    /// <param name="indentSize">Specifies the number of spaces to indent each level when symbols are not used. Default is 2.</param>
    /// <returns>A string representation of the tree structure.</returns>
    public static string Print(ITreeNode root, bool showId = true, bool markError = false, bool useSymbols = true, int indentSize = 2)
    {
        var sb = new StringBuilder();

        // Print the root node without any prefix or symbol
        string rootOutput = root.GetOutputContent(showId);

        // If marking errors is enabled, and the root node is an HTMLElement with spelling errors, mark it
        if (markError && root is HTMLElement rootElement && rootElement.IsSpellError)
        {
            rootOutput = "[X] " + rootOutput;
        }

        sb.AppendLine(rootOutput);

        var children = root.Children.ToList();
        for (int i = 0; i < children.Count; i++)
        {
            bool isLast = (i == children.Count - 1);
            PrintTreeRecursive(children[i], sb, "", isLast, showId, markError, useSymbols, indentSize);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Recursively prints the tree structure of the given node and its children.
    /// </summary>
    /// <param name="node">The current node to be printed.</param>
    /// <param name="sb">The StringBuilder used to accumulate the output.</param>
    /// <param name="prefix">The prefix to be added before the node output (for tree hierarchy).</param>
    /// <param name="isLast">Specifies if the current node is the last child in its parent's list of children.</param>
    /// <param name="showId">Specifies whether to show the ID of the node.</param>
    /// <param name="markError">Specifies whether to mark spelling errors in nodes.</param>
    /// <param name="useSymbols">Specifies whether to use symbols (├── and └──) for hierarchy representation.</param>
    /// <param name="indentSize">The number of spaces to indent when symbols are not used.</param>
    private static void PrintTreeRecursive(ITreeNode node, StringBuilder sb, string prefix, bool isLast, bool showId, bool markError, bool useSymbols, int indentSize)
    {
        // Get the output content for the current node
        string output = node.GetOutputContent(showId);

        // If marking errors is enabled and the node is an HTMLElement with spelling errors, mark it
        if (markError && node is HTMLElement element && element.IsSpellError)
        {
            output = "[X] " + output;
        }

        // Add the appropriate prefix and tree symbol based on whether symbols are enabled
        if (useSymbols)
        {
            sb.Append(prefix);
            sb.Append(isLast ? "└── " : "├── ");
        }
        else
        {
            sb.Append(prefix);
            sb.Append(new string(' ', indentSize));
        }

        sb.AppendLine(output);

        // If the node has content (in case of HTMLElement), print it as a separate child node
        if (node is HTMLElement htmlElement && !string.IsNullOrEmpty(htmlElement.Content))
        {
            string contentOutput = htmlElement.Content;

            // If marking errors is enabled and the content has spelling errors, mark it
            if (markError && htmlElement.IsSpellError)
            {
                contentOutput = "[X] " + contentOutput;
            }

            // Adjust the prefix for content indentation
            string contentPrefix = prefix + (isLast ? "    " : "│   ");

            // Print content with or without tree symbols
            if (useSymbols)
            {
                sb.AppendLine($"{contentPrefix}└── {contentOutput}");
            }
            else
            {
                sb.AppendLine($"{contentPrefix}{new string(' ', indentSize)}{contentOutput}");
            }
        }

        // Recursively print the children of the current node
        var children = node.Children.ToList();

        for (int i = 0; i < children.Count; i++)
        {
            bool childIsLast = (i == children.Count - 1);
            string newPrefix = prefix;

            // Adjust the prefix for the next level
            if (useSymbols)
            {
                newPrefix += isLast ? "    " : "│   ";
            }
            else
            {
                newPrefix += new string(' ', indentSize);
            }

            PrintTreeRecursive(children[i], sb, newPrefix, childIsLast, showId, markError, useSymbols, indentSize);
        }
    }
}
