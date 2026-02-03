namespace OpenRouter.SDK.Models;

/// <summary>
/// Represents a parameter that can be either a static value or dynamically resolved based on context
/// </summary>
/// <typeparam name="T">The type of the parameter value</typeparam>
public class DynamicParameter<T>
{
    private readonly T? _staticValue;
    private readonly Func<TurnContext, T>? _syncResolver;
    private readonly Func<TurnContext, Task<T>>? _asyncResolver;
    private readonly bool _isStatic;

    /// <summary>
    /// Create a dynamic parameter from a static value
    /// </summary>
    public DynamicParameter(T value)
    {
        _staticValue = value;
        _isStatic = true;
    }

    /// <summary>
    /// Create a dynamic parameter from a synchronous resolver function
    /// </summary>
    public DynamicParameter(Func<TurnContext, T> resolver)
    {
        _syncResolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _isStatic = false;
    }

    /// <summary>
    /// Create a dynamic parameter from an asynchronous resolver function
    /// </summary>
    public DynamicParameter(Func<TurnContext, Task<T>> asyncResolver)
    {
        _asyncResolver = asyncResolver ?? throw new ArgumentNullException(nameof(asyncResolver));
        _isStatic = false;
    }

    /// <summary>
    /// Resolve the parameter value based on the current context
    /// </summary>
    public async Task<T> ResolveAsync(TurnContext context)
    {
        if (_isStatic)
        {
            return _staticValue!;
        }

        if (_asyncResolver != null)
        {
            return await _asyncResolver(context);
        }

        if (_syncResolver != null)
        {
            return _syncResolver(context);
        }

        throw new InvalidOperationException("Dynamic parameter has no resolver");
    }

    /// <summary>
    /// Implicit conversion from static value to DynamicParameter
    /// </summary>
    public static implicit operator DynamicParameter<T>(T value) => new(value);

    /// <summary>
    /// Implicit conversion from sync resolver to DynamicParameter
    /// </summary>
    public static implicit operator DynamicParameter<T>(Func<TurnContext, T> resolver) => new(resolver);

    /// <summary>
    /// Implicit conversion from async resolver to DynamicParameter
    /// </summary>
    public static implicit operator DynamicParameter<T>(Func<TurnContext, Task<T>> asyncResolver) => new(asyncResolver);
}

/// <summary>
/// Request with support for dynamic parameter resolution
/// </summary>
public class DynamicBetaResponsesRequest
{
    /// <summary>
    /// Model to use (can be dynamic based on context)
    /// </summary>
    public DynamicParameter<string>? Model { get; set; }

    /// <summary>
    /// Temperature setting (can be dynamic based on context)
    /// </summary>
    public DynamicParameter<double?>? Temperature { get; set; }

    /// <summary>
    /// Max output tokens (can be dynamic based on context)
    /// </summary>
    public DynamicParameter<int?>? MaxOutputTokens { get; set; }

    /// <summary>
    /// Top P setting (can be dynamic based on context)
    /// </summary>
    public DynamicParameter<double?>? TopP { get; set; }

    /// <summary>
    /// Input messages (typically static, but can be dynamic)
    /// </summary>
    public DynamicParameter<List<ResponsesInputItem>>? Input { get; set; }

    /// <summary>
    /// System instructions (can be dynamic based on context)
    /// </summary>
    public DynamicParameter<string?>? Instructions { get; set; }

    /// <summary>
    /// Tools available (can be dynamic - add/remove tools based on context)
    /// </summary>
    public DynamicParameter<List<ResponsesFunctionTool>?>? Tools { get; set; }

    /// <summary>
    /// Stream responses
    /// </summary>
    public bool Stream { get; set; }

    /// <summary>
    /// Resolve all dynamic parameters to a static BetaResponsesRequest
    /// </summary>
    public async Task<BetaResponsesRequest> ResolveAsync(TurnContext context)
    {
        // Resolve Input and simplify if it's a single text item
        object? resolvedInput = null;
        if (Input != null)
        {
            var inputList = await Input.ResolveAsync(context);
            // If it's a single text item, convert to string for simplicity
            if (inputList?.Count == 1 && inputList[0].Type == "text" && !string.IsNullOrEmpty(inputList[0].Text))
            {
                resolvedInput = inputList[0].Text;
            }
            else
            {
                resolvedInput = inputList;
            }
        }
        
        var request = new BetaResponsesRequest
        {
            Stream = Stream,
            Store = false, // Required for /responses endpoint
            ServiceTier = "auto", // Required for /responses endpoint
            Model = Model != null ? await Model.ResolveAsync(context) : null!,
            Temperature = Temperature != null ? await Temperature.ResolveAsync(context) : null,
            MaxOutputTokens = MaxOutputTokens != null ? await MaxOutputTokens.ResolveAsync(context) : null,
            TopP = TopP != null ? await TopP.ResolveAsync(context) : null,
            Input = resolvedInput!,
            Instructions = Instructions != null ? await Instructions.ResolveAsync(context) : null,
            Tools = Tools != null ? await Tools.ResolveAsync(context) : null
        };

        return request;
    }
}
