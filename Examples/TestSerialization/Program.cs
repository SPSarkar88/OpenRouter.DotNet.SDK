using OpenRouter.SDK.Models;
using System.Text.Json;

// Use the EXACT same JSON options as the SDK
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true,
    Converters = 
    { 
        new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase) 
    }
};

Console.WriteLine("=== Testing Role Customization ===\n");

// 1. Default roles
Console.WriteLine("1. Default roles:");
var defaultMessages = new List<Message>
{
    new SystemMessage { Content = "You are helpful" },
    new UserMessage { Content = "Hello" }
};
var request1 = new ChatCompletionRequest
{
    Model = "test-model",
    Messages = defaultMessages
};
Console.WriteLine(JsonSerializer.Serialize(request1, jsonOptions));

// 2. Custom roles set at runtime
Console.WriteLine("\n2. Custom roles (user-defined at runtime):");
var customMessages = new List<Message>
{
    new SystemMessage { Role = "custom-system", Content = "Custom system message" },
    new UserMessage { Role = "custom-user", Content = "Custom user message" },
    new AssistantMessage { Role = "ai-assistant", Content = "Custom assistant response" }
};
var request2 = new ChatCompletionRequest
{
    Model = "test-model",
    Messages = customMessages
};
Console.WriteLine(JsonSerializer.Serialize(request2, jsonOptions));

Console.WriteLine("\n✓ Users can now set custom role values at runtime!");
