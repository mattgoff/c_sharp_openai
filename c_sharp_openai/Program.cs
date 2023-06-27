using System.Net.Http.Headers;
using System.Text.Json;
using c_sharp_openai.Models;
using Microsoft.Extensions.Configuration;

namespace c_sharp_openai
{
    public class ChatGptClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new();
        private readonly string _gptModel;

        private ChatGptClient(string apiKey, string gptModel)
        {
            _gptModel = gptModel;
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://api.openai.com/v1/chat/completions")
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            _jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
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
                    model = _gptModel,
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

                using var response = await _httpClient.PostAsync("", content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStreamAsync();
                var chatResponse =
                    (await JsonSerializer.DeserializeAsync<ChatResponse>(jsonResponse, _jsonSerializerOptions));
                return chatResponse!.choices[0].message.content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return "Sorry, there was an error processing your request. Please try again later.";
            }
        }

        private class Program
        {
            private static async Task Main()
            {
                var configuration = GetConfiguration();
                var apiKey = configuration["ChatGPTSecrets:FirstAPIKey"] ?? 
                                throw new InvalidOperationException("API Key not found in configuration.");
                string gptModel = configuration["ChatGptConfig:Model"] ??
                                throw new InvalidOperationException("GPT Model not found in configuration.");
                
                using var chatGptClient = new ChatGptClient(apiKey, gptModel);
                Console.WriteLine("Welcome to the ChatGPT ChatBot! Type 'exit' to quit.");
                
                while (true)
                {
                    DisplayInstructions();
                    var input = GetUserInput();

                    if (input.Equals("EXIT_CHAT", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                    
                    if (input.Length > 0)
                    {
                        Console.WriteLine("Sending to openai...");
                        string response = await chatGptClient.SendMessageAsync(input);
                        DisplayChatBotResponse(response);
                    }
                }
            }

            private static IConfiguration GetConfiguration()
            {
                var configBuilder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .AddUserSecrets<Program>();

                return configBuilder.Build();
            }

            private static void DisplayInstructions()
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Type your message and press ENTER.");
                Console.WriteLine("Type 'SEND_CHAT' and press ENTER to send your message.");
                Console.WriteLine("Type 'EXIT_CHAT' and press ENTER to quit the application.");
                Console.Write("You: ");
                Console.ResetColor();
            }

            private static string GetUserInput()
            {
                string input = "";
                string? readIn = "";
                while (readIn != "SEND_CHAT" && readIn != "EXIT_CHAT")
                {
                    readIn = Console.ReadLine();
                    if (readIn!.ToUpper() == "EXIT_CHAT")
                    {
                        return "EXIT_CHAT";
                    }

                    if (readIn.ToUpper() != "SEND_CHAT")
                    {
                        input += readIn + "\n";
                    }
                }

                return input.Trim();
            }

            private static void DisplayChatBotResponse(string response)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("ChatBot: ");
                Console.ResetColor();
                Console.WriteLine(response);
            }
        }
    }
}



