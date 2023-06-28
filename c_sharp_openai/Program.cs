using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using c_sharp_openai.Models;
using Microsoft.Extensions.Configuration;

namespace c_sharp_openai
{
    public class ChatGptClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new();
        private ChatObjectClass ChatObject { get; }

        private ChatGptClient(string apiKey, string gptModel, string gptUrl)
        {
            _httpClient = CreateHttpClient(apiKey, gptUrl);
            _jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            ChatObject = CreateChatObjectClass(gptModel);
        }

        private static HttpClient CreateHttpClient(string apiKey, string gptUrl)
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri(gptUrl)
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private ChatObjectClass CreateChatObjectClass(string gptModel)
        {
            return new ChatObjectClass()
            {
                Model = gptModel,
                Temperature = 0.7,
                Messages = Array.Empty<Message>()
            };
        }

        public void Dispose() => _httpClient.Dispose();
        
        private void UpdateJsonObject(string message, string role)
        {
            ChatObject.Messages = ChatObject.Messages.Append(new Message()
            {
                Role = role,
                Content = message
            }).ToArray();
        }
        
        private async Task<string> SendMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return MissingInputMessage();
            try
            {
                UpdateJsonObject(message, "user");
                using var response = await GetResponseFromOpenAi();
                response.EnsureSuccessStatusCode();
                var chatResponse = await ParseResponse(response);
                return ExtractMessageInResponse(chatResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return ErrorMessage();
            }
        }

        private Task<HttpResponseMessage> GetResponseFromOpenAi()
        {
            var jsonString = JsonSerializer.Serialize(ChatObject, _jsonSerializerOptions);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
            return _httpClient.PostAsync("", content);
        }

        private async Task<ChatResponse> ParseResponse(HttpResponseMessage response)
        {
            var jsonResponse = await response.Content.ReadAsStreamAsync();
            return (await JsonSerializer.DeserializeAsync<ChatResponse>(jsonResponse, _jsonSerializerOptions)) ?? throw new InvalidOperationException();
        }

        private string ExtractMessageInResponse(ChatResponse chatResponse)
        {
            if (chatResponse?.Choices != null)
            {
                var responseMessage = chatResponse?.Choices[0]?.Message;
                if (responseMessage is not {Content: not null, Role: not null})
                    return NoResponseMessage();
                
                UpdateJsonObject(responseMessage.Content, responseMessage.Role);
                return responseMessage.Content;
            }

            return NoResponseMessage();
        }
        
        private string MissingInputMessage() => "I didn't receive any input. Please try again!";
        private string ErrorMessage() => "Sorry, there was an error processing your request. Please try again later.";
        private string NoResponseMessage() => "No response received please try again later.";
        
        private class Program
        {
            private static async Task Main()
            {
                var configuration = GetConfiguration();
                var apiKey = GetApiOrThrow(configuration);
                string gptModel = GetGptModelOrThrow(configuration);
                string gptUrl = GetGptUrlOrThrow(configuration);
                
                using var chatGptClient = new ChatGptClient(apiKey, gptModel, gptUrl);
                DisplayInstructions();
                
                while (true)
                {
                    DisplayPrompt();
                    var input = GetUserInput();

                    if (IsExitCommand(input)) break;
                    
                    var responseText = await ProcessInput(chatGptClient, input);
                    DisplayChatBotResponse(responseText);
                }
            }

            private static IConfiguration GetConfiguration() => new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>()
                .Build();

            private static string GetApiOrThrow(IConfiguration configuration) =>
                configuration["ChatGPTSecrets:FirstAPIKey"] ?? 
                throw new InvalidOperationException("API Key not found in configuration.");
            
            private static string GetGptModelOrThrow(IConfiguration configuration) =>
                configuration["ChatGptConfig:Model"] ??
                throw new InvalidOperationException("GPT Model not found in configuration.");

            private static string GetGptUrlOrThrow(IConfiguration configuration) =>
                configuration["ChatGptConfig:URL"] ??
                throw new InvalidOperationException("GPT URL not found in configuration");
            
            private static void DisplayInstructions()
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Type your message and press ENTER.");
                Console.WriteLine("Type 'EXIT_CHAT' and press ENTER to quit the application.");
                Console.ResetColor();
            }
            
            private static void DisplayPrompt()
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("You: ");
                Console.ResetColor();
            }

            private static string GetUserInput() => Console.ReadLine() ?? string.Empty;
            private static bool IsExitCommand(string input) => input.Equals("EXIT_CHAT", StringComparison.OrdinalIgnoreCase);
            private static async Task<string> ProcessInput(ChatGptClient chatGptClient, string input) =>
                input.Length > 0 ? await chatGptClient.SendMessageAsync(input) : string.Empty;
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



