namespace c_sharp_openai.Models;

public class ChatResponse
{
    public Error error { get; set; }
    public string id { get; set; }
    public int created { get; set; }
    public string model { get; set; }
    public Usage usage { get; set; }
    public Choices[] choices { get; set; }
}

public class Usage
{
    public int prompt_tokens { get; set; }
    public int completion_tokens { get; set; }
    public int total_tokens { get; set; }
}

public class Choices
{
    public Message message { get; set; }
    public string finish_reason { get; set; }
    public int index { get; set; }
}

public class Message
{
    public string role { get; set; }
    public string content { get; set; }
}

public class Error
{
    public string message { get; set; }
    public string type { get; set; }
    public object param { get; set; }
    public object code { get; set; }
}

