using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

public class HtmlEditorTests
{
    private HtmlEditor editor;
    private HistoryManager historyManager;
    private static readonly HttpClient client = new HttpClient();

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlEditorTests"/> class.
    /// </summary>
    public HtmlEditorTests()
    {
        editor = new HtmlEditor();
        historyManager = new HistoryManager();
    }

    /// <summary>
    /// Tests the initialization of the HTML editor.
    /// </summary>
    [Fact]
    public void TestInitialization()
    {
        // Act
        editor.Init();

        // Assert
        Assert.NotNull(editor.RootHtml.FindById("html"));
        Assert.NotNull(editor.RootHtml.FindById("head"));
        Assert.NotNull(editor.RootHtml.FindById("body"));
    }

    /// <summary>
    /// Tests appending an element to the HTML document.
    /// </summary>
    [Fact]
    public void TestAppendElement()
    {
        // Arrange
        editor.Init();

        // Act
        editor.Append("div", "div1", "body", "This is a div element.");

        // Assert
        var divElement = editor.RootHtml.FindById("div1");
        Assert.NotNull(divElement);
        Assert.Equal("div", divElement.TagName);
        Assert.Equal("This is a div element.", divElement.Content);
    }

    /// <summary>
    /// Tests inserting an element into another element.
    /// </summary>
    [Fact]
    public void TestInsertElement()
    {
        // Arrange
        editor.Init();
        editor.Append("div", "div1", "body", "This is a div element.");

        // Act
        editor.Insert("p", "para1", "div1", "This is a paragraph.");

        // Assert
        var paraElement = editor.RootHtml.FindById("para1");
        Assert.NotNull(paraElement);
        Assert.Equal("p", paraElement.TagName);
        Assert.Equal("This is a paragraph.", paraElement.Content);
    }

    /// <summary>
    /// Tests editing the text content of an element.
    /// </summary>
    [Fact]
    public void TestEditText()
    {
        // Arrange
        editor.Init();
        editor.Append("p", "para1", "body", "Initial text.");

        // Act
        editor.EditText("para1", "Updated text.");

        // Assert
        var paraElement = editor.RootHtml.FindById("para1");
        Assert.Equal("Updated text.", paraElement?.Content);
    }

    /// <summary>
    /// Tests changing the ID of an element.
    /// </summary>
    [Fact]
    public void TestEditId()
    {
        // Arrange
        editor.Init();
        editor.Append("div", "div1", "body", "This is a div element.");

        // Act
        editor.EditId("div1", "newDivId");

        // Assert
        var newElement = editor.RootHtml.FindById("newDivId");
        Assert.NotNull(newElement);
        Assert.Equal("div", newElement.TagName);
    }

    /// <summary>
    /// Tests deleting an element from the HTML document.
    /// </summary>
    [Fact]
    public void TestDeleteElement()
    {
        // Arrange
        editor.Init();
        editor.Append("div", "div1", "body", "This is a div element.");

        // Act
        editor.Delete("div1");

        // Assert
        var divElement = editor.RootHtml.FindById("div1");
        Assert.Null(divElement);  // Element should be deleted
    }

    /// <summary>
    /// Tests undo and redo operations.
    /// </summary>
    [Fact]
    public void TestUndoRedo()
    {
        // Arrange
        editor.Init();
        editor.Append("p", "para1", "body", "Initial paragraph.");

        // Act
        historyManager.SaveState(editor.RootHtml);
        editor.EditText("para1", "Modified paragraph.");
        historyManager.SaveState(editor.RootHtml);

        var modifiedContent = editor.RootHtml.FindById("para1")!.Content;

        editor.RootHtml = historyManager.Undo();
        var undoneContent = editor.RootHtml.FindById("para1")!.Content;

        editor.RootHtml = historyManager.Redo();
        var redoneContent = editor.RootHtml.FindById("para1")!.Content;

        // Assert
        Assert.Equal("Modified paragraph.", modifiedContent);
        Assert.Equal("Initial paragraph.", undoneContent);
        Assert.Equal("Modified paragraph.", redoneContent);
    }
}
