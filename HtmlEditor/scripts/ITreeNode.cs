/// <summary>
/// Represents a node in a tree structure, where each node can have child nodes.
/// </summary>
/// <remarks>
/// This interface defines the contract for tree nodes, which allows for accessing child nodes 
/// and generating some output content for each node.
/// </remarks>
public interface ITreeNode
{
    /// <summary>
    /// Gets the collection of child nodes for the current node.
    /// </summary>
    /// <value>
    /// A collection of child nodes, or an empty collection if there are no children.
    /// </value>
    IEnumerable<ITreeNode> Children { get; }

    /// <summary>
    /// Retrieves the output content for the node, optionally including its ID.
    /// </summary>
    /// <param name="showId">Specifies whether the ID of the node should be included in the output.</param>
    /// <returns>A string representing the output content of the node.</returns>
    string GetOutputContent(bool showId);
}
