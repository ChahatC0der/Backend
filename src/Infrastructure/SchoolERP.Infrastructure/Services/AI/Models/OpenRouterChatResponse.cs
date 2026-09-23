namespace SchoolERP.Infrastructure.Services.AI.Models;

public sealed class OpenRouterChatResponse
{
    public List<Choice>? Choices { get; set; }

    public Usage? Usage { get; set; }
}

public sealed class Choice
{
    public Message? Message { get; set; }

    public string? FinishReason { get; set; }
}

public sealed class Message
{
    public string? Role { get; set; }

    public string? Content { get; set; }
}

public sealed class Usage
{
    public int PromptTokens { get; set; }

    public int CompletionTokens { get; set; }

    public int TotalTokens { get; set; }
}