using System.Text.Json.Serialization;

namespace c_sharp_openai.Models;

public class ChatResponse
{
    [JsonPropertyName("error")]
    public Error? Error { get; set; }
    
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("created")]
    public int Created { get; set; }
    
    [JsonPropertyName("model")]
    public string? Model { get; set; }
    
    [JsonPropertyName("usage")]
    public Usage? Usage { get; set; }
    
    [JsonPropertyName("choices")]
    public Choices[]? Choices { get; set; }
}

public class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; init; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; init; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; init; }
}

public class Choices
{
    [JsonPropertyName("message")]
    public Message? Message { get; init; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; init; }

    [JsonPropertyName("index")]
    public int Index { get; init; }
}

public class Message
{
    [JsonPropertyName("role")]
    public string? Role { get; init; }

    [JsonPropertyName("content")]
    public string? Content { get; init; }
}

public class Error
{        
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }
        
    [JsonPropertyName("param")]
    public object? Param { get; init; }
        
    [JsonPropertyName("code")]
    public object? Code { get; init; }
}

