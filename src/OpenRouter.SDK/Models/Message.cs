using System.Text.Json.Serialization;

namespace OpenRouter.SDK.Models;

/// <summary>
/// Represents a message in a chat completion.
/// </summary>
[JsonConverter(typeof(MessageJsonConverter))]
public abstract class Message
{
    /// <summary>
    /// Gets or sets the role of the message author.
    /// </summary>
    public virtual string Role { get; set; } = string.Empty;
}

/// <summary>
/// Represents a system message that sets the behavior of the assistant.
/// </summary>
public class SystemMessage : Message
{
    /// <summary>
    /// Gets or sets the role of the message (defaults to "system").
    /// </summary>
    [JsonPropertyName("role")]
    public override string Role { get; set; } = "system";

    /// <summary>
    /// Gets or sets the content of the system message.
    /// </summary>
    [JsonPropertyName("content")]
    public required string Content { get; set; }
}

/// <summary>
/// Represents a user message in a conversation.
/// </summary>
public class UserMessage : Message
{
    /// <summary>
    /// Gets or sets the role of the message (defaults to "user").
    /// </summary>
    [JsonPropertyName("role")]
    public override string Role { get; set; } = "user";

    /// <summary>
    /// Gets or sets the content of the user message.
    /// Can be a string or an array of content items for multimodal messages.
    /// </summary>
    [JsonPropertyName("content")]
    public required object Content { get; set; } // string or ChatMessageContentItem[]

    /// <summary>
    /// Gets or sets an optional name for the message author.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

/// <summary>
/// Represents an assistant message in a conversation.
/// </summary>
public class AssistantMessage : Message
{
    /// <summary>
    /// Gets or sets the role of the message (defaults to "assistant").
    /// </summary>
    [JsonPropertyName("role")]
    public override string Role { get; set; } = "assistant";

    /// <summary>
    /// Gets or sets the content of the assistant's message.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets tool calls made by the assistant.
    /// </summary>
    [JsonPropertyName("tool_calls")]
    public List<ToolCall>? ToolCalls { get; set; }

    /// <summary>
    /// Gets or sets the reasoning content (for models that support reasoning).
    /// </summary>
    [JsonPropertyName("reasoning_content")]
    public string? ReasoningContent { get; set; }
}

/// <summary>
/// Represents a tool/function message containing the result of a tool call.
/// </summary>
public class ToolMessage : Message
{
    /// <summary>
    /// Gets or sets the role of the message (defaults to "tool").
    /// </summary>
    [JsonPropertyName("role")]
    public override string Role { get; set; } = "tool";

    /// <summary>
    /// Gets or sets the content of the tool's response.
    /// </summary>
    [JsonPropertyName("content")]
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the ID of the tool call this message is responding to.
    /// </summary>
    [JsonPropertyName("tool_call_id")]
    public required string ToolCallId { get; set; }
}

/// <summary>
/// Represents a tool call made by the assistant.
/// </summary>
public class ToolCall
{
    /// <summary>
    /// Gets or sets the ID of the tool call.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the type of tool call (typically "function").
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <summary>
    /// Gets or sets the function call details.
    /// </summary>
    [JsonPropertyName("function")]
    public required FunctionCall Function { get; set; }
}

/// <summary>
/// Represents a function call within a tool call.
/// </summary>
public class FunctionCall
{
    /// <summary>
    /// Gets or sets the name of the function to call.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the JSON-formatted arguments for the function.
    /// </summary>
    [JsonPropertyName("arguments")]
    public required string Arguments { get; set; }
}

/// <summary>
/// Content items for multimodal messages.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextContentItem), "text")]
[JsonDerivedType(typeof(ImageContentItem), "image_url")]
public abstract class ChatMessageContentItem
{
    /// <summary>
    /// Gets the type of the content item.
    /// </summary>
    [JsonPropertyName("type")]
    public abstract string Type { get; }
}

/// <summary>
/// Represents text content in a message.
/// </summary>
public class TextContentItem : ChatMessageContentItem
{
    /// <summary>
    /// Gets the type ("text").
    /// </summary>
    public override string Type => "text";

    /// <summary>
    /// Gets or sets the text content.
    /// </summary>
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

/// <summary>
/// Represents image content in a message.
/// </summary>
public class ImageContentItem : ChatMessageContentItem
{
    /// <summary>
    /// Gets the type ("image_url").
    /// </summary>
    public override string Type => "image_url";

    /// <summary>
    /// Gets or sets the image URL.
    /// </summary>
    [JsonPropertyName("image_url")]
    public required ImageUrl ImageUrl { get; set; }
}

/// <summary>
/// Represents an image URL with optional detail level.
/// </summary>
public class ImageUrl
{
    /// <summary>
    /// Gets or sets the URL of the image (can be a URL or base64-encoded data URI).
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    /// <summary>
    /// Gets or sets the detail level for image processing.
    /// </summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }
}
