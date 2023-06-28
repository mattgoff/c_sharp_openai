namespace c_sharp_openai.Models;

public class ChatObject
{
    public string model { get; set; }
    public double temperature { get; set; }
    public Message[] messages { get; set; }
}