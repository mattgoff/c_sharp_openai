using c_sharp_openai.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;

namespace c_sharp_openai
{
    public class ChatGptClient
    {
        private readonly string _apiKey;
        private readonly RestClient _client;
        private RestResponse _response;
        

        private ChatGptClient(string apiKey)
        {
            _apiKey = apiKey;
            _response = null!;
            _client = new RestClient("https://api.openai.com/v1/chat/completions");
            // _client = new RestClient("https://api.openai.com/v1/engines/text-davinci-003/completions");
        }

        private string SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "Sorry, I didn't receive any input. Please try again!";
            }

            try
            {
                var request = new RestRequest("", Method.Post);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", $"Bearer {_apiKey}");

                var jsonObject = new
                {
                    model = "gpt-3.5-turbo-16k",
                    // model = "text-davinci-003",
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

                // Using python write a function that will solve for the nth number in the fibonacci sequence
                request.AddJsonBody(JsonConvert.SerializeObject(jsonObject));
                _response = _client.Execute(request);
                var jsonResponse = JsonConvert.DeserializeObject<ChatResponse>(_response.Content ?? string.Empty);
                return jsonResponse?.choices[0].message.content ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return "Sorry, there was an error processing your request. Please try again later.";
            }
        }

        private class Program
        {
            private static void Main(string[] args)
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
                    string response = chatGptClient.SendMessage(input);

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("Chatbot: ");
                    Console.ResetColor();
                    Console.WriteLine(response);
                }
            }
        }
    }
}



