using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using c_sharp_openai.Models;
using Microsoft.Extensions.Configuration;

namespace c_sharp_openai
{
    public class ChatGptClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new();
        
        public ChatGptClient(string apiKey)
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://api.openai.com/v1/chat/completions")
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            _jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }
        
        private async Task<string> SendMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "Sorry, I didn't receive any input. Please try again!";
            }

            try
            {
                var jsonObject = new
                {
                    model = "gpt-3.5-turbo-16k",
                    temperature = 0.7,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = message
                        }
                    }
                };

                var jsonString = JsonSerializer.Serialize(jsonObject, _jsonSerializerOptions);
                var content = new StringContent(jsonString);
                content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

                var response = await _httpClient.PostAsync("", content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStreamAsync();
                var chatResponse =
                    (await JsonSerializer.DeserializeAsync<ChatResponse>(jsonResponse, _jsonSerializerOptions));
                return chatResponse.choices[0].message.content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return "Sorry, there was an error processing your request. Please try again later.";
            }
        }

        private class Program
        {
            private static async Task Main(string[] args)
            {
                var configBuilder = new ConfigurationBuilder()
                    .AddUserSecrets<Program>();
                var configuration = configBuilder.Build();
                var apiKey = configuration["ChatGPTSecrets:FirstAPIKey"] ?? throw new InvalidOperationException();
                var chatGptClient = new ChatGptClient(apiKey);

                Console.WriteLine("Welcome to the ChatGPT chatbot! Type 'exit' to quit.");

                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("EXIT_CHAT and SEND_CHAT to do the thing");
                    Console.Write("You: ");
                    Console.ResetColor();
                    // string input = Console.ReadLine() ?? string.Empty;
                    string input = "";
                    string? readIn = "";
                    while (readIn != "SEND_CHAT" && readIn != "EXIT_CHAT")
                    {
                        readIn = Console.ReadLine();
                        if (readIn.ToUpper() == "EXIT_CHAT")
                        {
                            Environment.Exit(0);
                        }
                        else if (readIn.ToUpper() != "SEND_CHAT")
                        {
                            input += readIn + "\n";
                        }
                    }

                    Console.WriteLine("Sending to openai...");
                    string response = await chatGptClient.SendMessageAsync(input);

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("Chatbot: ");
                    Console.ResetColor();
                    Console.WriteLine(response);
                }
            }
        }
    }
}



