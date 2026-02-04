using System.Text.Json;
using OpenRouter.SDK.Models;

namespace OpenRouter.SDK.Services;

/// <summary>
/// Service for executing individual tools with validation and error handling
/// Handles both regular tools and generator tools (for future extensibility)
/// </summary>
public class ToolExecutor
{
    /// <summary>
    /// Find a tool by name in a collection of tools
    /// </summary>
    /// <param name="tools">Collection of tools to search</param>
    /// <param name="toolName">Name of the tool to find</param>
    /// <returns>The matching tool, or null if not found</returns>
    public static ITool? FindToolByName(IEnumerable<ITool> tools, string toolName)
    {
        return tools.FirstOrDefault(t => t.Name == toolName);
    }

    /// <summary>
    /// Execute a tool with the given tool call and context
    /// </summary>
    /// <param name="tool">The tool to execute</param>
    /// <param name="toolCall">The tool call from the API response</param>
    /// <param name="context">Turn context with conversation state</param>
    /// <returns>Tool execution result</returns>
    public static async Task<ToolExecutionResult> ExecuteToolAsync(
        ITool tool,
        FunctionToolCall toolCall,
        TurnContext context)
    {
        try
        {
            // Check if tool has execute function
            if (!tool.HasExecuteFunction)
            {
                return new ToolExecutionResult
                {
                    ToolCallId = toolCall.Id,
                    ToolName = toolCall.Name,
                    Result = null,
                    Error = new InvalidOperationException($"Tool '{toolCall.Name}' has no execute function")
                };
            }

            // Parse tool call arguments
            object? arguments;
            try
            {
                using var doc = JsonDocument.Parse(toolCall.Arguments);
                arguments = doc.RootElement.Clone();
            }
            catch (JsonException ex)
            {
                return new ToolExecutionResult
                {
                    ToolCallId = toolCall.Id,
                    ToolName = toolCall.Name,
                    Result = null,
                    Error = new InvalidOperationException($"Failed to parse tool call arguments: {ex.Message}", ex)
                };
            }

            // Execute tool using reflection to call the generic method
            var toolType = tool.GetType();
            
            // Check if it's a Tool<TInput, TOutput>
            if (toolType.IsGenericType && toolType.GetGenericTypeDefinition() == typeof(Tool<,>))
            {
                var genericArgs = toolType.GetGenericArguments();
                var inputType = genericArgs[0];
                var outputType = genericArgs[1];

                // Deserialize arguments to TInput
                object? input;
                try
                {
                    var jsonElement = (JsonElement)arguments;
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    input = JsonSerializer.Deserialize(jsonElement.GetRawText(), inputType, options);
                    
                    if (input == null)
                    {
                        return new ToolExecutionResult
                        {
                            ToolCallId = toolCall.Id,
                            ToolName = toolCall.Name,
                            Result = null,
                            Error = new InvalidOperationException($"Failed to deserialize tool input for '{toolCall.Name}'")
                        };
                    }
                }
                catch (JsonException ex)
                {
                    return new ToolExecutionResult
                    {
                        ToolCallId = toolCall.Id,
                        ToolName = toolCall.Name,
                        Result = null,
                        Error = new InvalidOperationException($"Failed to deserialize tool input: {ex.Message}", ex)
                    };
                }

                // Call ExecuteAsync method
                var executeMethod = toolType.GetMethod("ExecuteAsync");
                if (executeMethod == null)
                {
                    return new ToolExecutionResult
                    {
                        ToolCallId = toolCall.Id,
                        ToolName = toolCall.Name,
                        Result = null,
                        Error = new InvalidOperationException($"ExecuteAsync method not found on tool '{toolCall.Name}'")
                    };
                }

                // Invoke the method
                var task = executeMethod.Invoke(tool, new[] { input, context }) as Task;
                if (task == null)
                {
                    return new ToolExecutionResult
                    {
                        ToolCallId = toolCall.Id,
                        ToolName = toolCall.Name,
                        Result = null,
                        Error = new InvalidOperationException($"ExecuteAsync did not return a Task for '{toolCall.Name}'")
                    };
                }

                await task.ConfigureAwait(false);

                // Get the result from the task
                var resultProperty = task.GetType().GetProperty("Result");
                var result = resultProperty?.GetValue(task);

                return new ToolExecutionResult
                {
                    ToolCallId = toolCall.Id,
                    ToolName = toolCall.Name,
                    Result = result
                };
            }

            return new ToolExecutionResult
            {
                ToolCallId = toolCall.Id,
                ToolName = toolCall.Name,
                Result = null,
                Error = new InvalidOperationException($"Unknown tool type for '{toolCall.Name}'")
            };
        }
        catch (Exception ex)
        {
            return new ToolExecutionResult
            {
                ToolCallId = toolCall.Id,
                ToolName = toolCall.Name,
                Result = null,
                Error = ex
            };
        }
    }

    /// <summary>
    /// Convert tools to API format for sending to OpenRouter
    /// </summary>
    /// <param name="tools">Collection of tools to convert</param>
    /// <returns>Array of tools in API format</returns>
    public static object[] ConvertToolsToApiFormat(IEnumerable<ITool> tools)
    {
        return tools.Select(t => t.ToApiFormat()).ToArray();
    }
}
