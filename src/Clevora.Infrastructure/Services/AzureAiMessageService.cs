using Azure.AI.OpenAI;
using Clevora.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace Clevora.Infrastructure.Services;

public class AzureAiMessageService(IConfiguration config) : IAiMessageService
{
    // Model routing: Fix/Rewrite → gpt-5-mini, Compose → gpt-5.6-luna
    private string ModelFor(AiOperation op) => op == AiOperation.Compose
        ? (config["AzureOpenAI:ModelCompose"] ?? "gpt-5.6-luna")
        : (config["AzureOpenAI:ModelAssist"] ?? "gpt-5-mini");

    public async Task<string> ProcessAsync(
        AiOperation operation,
        string text,
        string? extraContext = null,
        CancellationToken ct = default)
    {
        var endpoint = config["AzureOpenAI:Endpoint"]
            ?? throw new InvalidOperationException("AzureOpenAI:Endpoint not configured.");
        var apiKey = config["AzureOpenAI:ApiKey"]
            ?? throw new InvalidOperationException("AzureOpenAI:ApiKey not configured.");

        var client = new AzureOpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey));
        var chatClient = client.GetChatClient(ModelFor(operation));

        var systemPrompt = operation switch
        {
            AiOperation.Fix =>
                "Kamu adalah asisten yang memperbaiki tata bahasa dan ejaan pesan WhatsApp. " +
                "Kembalikan hanya pesan yang sudah diperbaiki, tanpa penjelasan.",
            AiOperation.Rewrite =>
                "Kamu adalah asisten yang menulis ulang pesan WhatsApp agar lebih profesional dan jelas. " +
                "Kembalikan hanya pesan yang sudah ditulis ulang.",
            AiOperation.Compose =>
                "Kamu adalah asisten penyusun pesan WhatsApp profesional. " +
                "Buat pesan WhatsApp yang efektif berdasarkan konteks berikut. " +
                "Kembalikan hanya pesan jadi, tanpa penjelasan.",
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };

        var userContent = string.IsNullOrWhiteSpace(extraContext)
            ? text
            : $"Konteks: {extraContext}\n\nTeks: {text}";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userContent)
        };

        var completion = await chatClient.CompleteChatAsync(messages, cancellationToken: ct);
        return completion.Value.Content[0].Text.Trim();
    }
}
