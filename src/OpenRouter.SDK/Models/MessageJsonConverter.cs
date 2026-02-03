using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenRouter.SDK.Models;

/// <summary>
/// Custom JSON converter for Message types that serializes role and content without type discriminator.
/// </summary>
public class MessageJsonConverter : JsonConverter<Message>
{
    public override Message? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("role", out var roleProp))
        {
            throw new JsonException("Message must have a 'role' property");
        }

        var role = roleProp.GetString();
        
        return role switch
        {
            "system" => JsonSerializer.Deserialize<SystemMessage>(root.GetRawText(), options),
            "user" => JsonSerializer.Deserialize<UserMessage>(root.GetRawText(), options),
            "assistant" => JsonSerializer.Deserialize<AssistantMessage>(root.GetRawText(), options),
            "tool" => JsonSerializer.Deserialize<ToolMessage>(root.GetRawText(), options),
            _ => throw new JsonException($"Unknown message role: {role}")
        };
    }

    public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
    {
        // Serialize the concrete type directly without type discriminator
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
