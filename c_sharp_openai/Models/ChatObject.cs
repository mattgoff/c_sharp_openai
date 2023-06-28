using System.Text.Json.Serialization;

namespace c_sharp_openai.Models;

public class ChatObjectClass
{
    [JsonPropertyName("model")]
    public string Model { get; set; }
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }
    [JsonPropertyName("messages")]
    public Message[] Messages { get; set; }
}