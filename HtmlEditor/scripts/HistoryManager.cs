using System;
using System.Collections.Generic;

/// <summary>
/// Manages the undo and redo history for HTML state changes, allowing the user to navigate through previous and future states.
/// </summary>
public class HistoryManager
{
    /// <summary>
    /// Stack to store the states for undo operations.
    /// </summary>
    private Stack<Html> undoStack = new Stack<Html>();

    /// <summary>
    /// Stack to store the states for redo operations.
    /// </summary>
    private Stack<Html> redoStack = new Stack<Html>();

    /// <summary>
    /// Saves the current HTML state for undo/redo operations.
    /// The state is pushed onto the undo stack and the redo stack is cleared.
    /// </summary>
    /// <param name="state">The HTML state to save.</param>
    public void SaveState(Html state)
    {
        undoStack.Push((Html)state.Clone());
        redoStack.Clear();
        // Console.WriteLine(state.PrintTree()); // Optional: Debugging/Logging state
    }

    /// <summary>
    /// Performs an undo operation, restoring the previous HTML state from the undo stack.
    /// </summary>
    /// <returns>The HTML state after the undo operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when there are no actions left to undo.</exception>
    public Html Undo()
    {
        if (undoStack.Count > 1)
        {
            redoStack.Push(undoStack.Pop());
            return (Html)undoStack.Peek().Clone();
        }
        throw new InvalidOperationException("No actions to undo.");
    }

    /// <summary>
    /// Performs a redo operation, restoring the next HTML state from the redo stack.
    /// </summary>
    /// <returns>The HTML state after the redo operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when there are no actions left to redo.</exception>
    public Html Redo()
    {
        if (redoStack.Count > 0)
        {
            var state = redoStack.Pop();
            undoStack.Push((Html)state.Clone());
            return (Html)state.Clone();
        }
        throw new InvalidOperationException("No actions to redo.");
    }
}
