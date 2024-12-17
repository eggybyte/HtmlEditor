using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#nullable disable

/// <summary>
/// Provides asynchronous spell checking for HTML content using LanguageTool API.
/// </summary>
/// <remarks>
/// This static class supports recursive checking and error context within HTML elements.
/// </remarks>
public static class SpellChecker
{
    /// <summary>
    /// The HTTP client used for making requests to the LanguageTool API.
    /// </summary>
    private static readonly HttpClient client = new HttpClient();

    /// <summary>
    /// Initiates spell checking on the given HTML element and its children.
    /// </summary>
    /// <param name="rootElement">The root HTML element to check.</param>
    /// <returns>A string report of spelling errors or a success message.</returns>
    /// <exception cref="HttpRequestException">Thrown on API call errors.</exception>
    public static async Task<string> CheckSpellingAsync(HTMLElement rootElement)
    {
        var errors = new StringBuilder();
        // Recursively check spelling in the root element and all its children
        await CheckSpellingRecursiveAsync(rootElement, errors);
        return errors.Length == 0 ? "No spelling errors found." : errors.ToString();
    }

    /// <summary>
    /// Recursively checks the text content of the specified HTML element and its children.
    /// </summary>
    /// <param name="element">The HTML element to check.</param>
    /// <param name="errors">A StringBuilder to accumulate spelling errors.</param>
    private static async Task CheckSpellingRecursiveAsync(HTMLElement element, StringBuilder errors)
    {
        // Check the current element if it contains text content
        if (!string.IsNullOrEmpty(element.Content))
        {
            var spellingErrors = await CheckTextWithLanguageTool(element.Content, element.Id);
            if (!string.IsNullOrEmpty(spellingErrors))
            {
                // If errors are found, append them to the errors collection and mark the element as having errors
                errors.AppendLine(spellingErrors);
                element.IsSpellError = true;
            }
            else
            {
                // No errors found, mark as error-free
                element.IsSpellError = false;
            }
        }
        else
        {
            // If no content, mark as error-free
            element.IsSpellError = false;
        }

        // Traverse child elements and recursively check them
        foreach (var child in element.Children)
        {
            await CheckSpellingRecursiveAsync(child, errors);
        }
    }

    /// <summary>
    /// Checks the text of a single HTML element using the LanguageTool API.
    /// </summary>
    /// <param name="text">The text to check for spelling errors.</param>
    /// <param name="elementId">The ID of the element for error reporting.</param>
    /// <returns>A string of spelling errors found, or an empty string if no errors.</returns>
    private static async Task<string> CheckTextWithLanguageTool(string text, string elementId)
    {
        var spellingErrors = new StringBuilder();

        try
        {
            // Prepare the request content for the LanguageTool API
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("text", text),
                new KeyValuePair<string, string>("language", "auto") // Automatically detect language
            });

            // Make the request to the LanguageTool API
            HttpResponseMessage response = await client.PostAsync("https://api.languagetool.org/v2/check", requestContent);
            response.EnsureSuccessStatusCode();

            // Deserialize the response
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var languageToolResponse = JsonSerializer.Deserialize<LanguageToolResponse>(jsonResponse);

            // If there are spelling matches, process them
            if (languageToolResponse?.Matches != null)
            {
                foreach (var match in languageToolResponse.Matches)
                {
                    // Only process "Spelling" and "Possible Typo" categories
                    if (match.Rule?.Category?.Name != "Spelling" && match.Rule?.Category?.Name != "Possible Typo")
                    {
                        continue;
                    }

                    // Extract suggestions for the misspelled word
                    var suggestions = match.Replacements.Take(10).Select(r => r.Value);
                    var suggestionsText = suggestions.Any() ? string.Join(", ", suggestions) : "No suggestions available";

                    // Extract the misspelled word and its context
                    string misspelledWord = "";
                    if (match.Context != null && match.Context.Offset + match.Context.Length <= match.Context.Text.Length)
                    {
                        misspelledWord = match.Context.Text.Substring(match.Context.Offset, match.Context.Length);
                    }

                    // Append detailed error information to the errors string
                    spellingErrors.AppendLine(
                        $"Element '{elementId}' - Misspelled word: '{misspelledWord}'\n" +
                        $"  Context: '{match.Context.Text}'\n" +
                        $"  Position: Offset {match.Context.Offset}, Length {match.Context.Length}\n" +
                        $"  Suggestions: {suggestionsText}\n"
                    );
                }
            }
        }
        catch (Exception ex)
        {
            // Log the error in case of an API or network failure
            Console.WriteLine($"Error during spell check for element '{elementId}': {ex.Message}");
        }

        // Return the collected spelling errors
        return spellingErrors.ToString();
    }

    /// <summary>
    /// Represents the response from the LanguageTool API.
    /// </summary>
    public class LanguageToolResponse
    {
        [JsonPropertyName("matches")]
        public List<Match> Matches { get; set; }
    }

    /// <summary>
    /// Represents a single match (spelling error) found by the LanguageTool API.
    /// </summary>
    public class Match
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("context")]
        public Context Context { get; set; }

        [JsonPropertyName("replacements")]
        public List<Replacement> Replacements { get; set; }

        [JsonPropertyName("rule")]
        public Rule Rule { get; set; }
    }

    /// <summary>
    /// Represents the context in which a spelling error occurred.
    /// </summary>
    public class Context
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("length")]
        public int Length { get; set; }
    }

    /// <summary>
    /// Represents a suggested replacement for a misspelled word.
    /// </summary>
    public class Replacement
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    /// <summary>
    /// Represents the rule that triggered a match.
    /// </summary>
    public class Rule
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("category")]
        public Category Category { get; set; }
    }

    /// <summary>
    /// Represents the category of a rule.
    /// </summary>
    public class Category
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}
