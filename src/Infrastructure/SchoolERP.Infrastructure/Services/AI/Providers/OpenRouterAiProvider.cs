using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SchoolERP.Application.AI.Configuration;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Infrastructure.Services.AI.Models;

namespace SchoolERP.Infrastructure.Services.AI.Providers;

public sealed class OpenRouterAiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;

    public OpenRouterAiProvider(
        HttpClient httpClient,
        IOptions<AiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public string Name => "OpenRouter";

    public async Task<AiChatResponse> ChatAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "AI API key is not configured.");
        }

        var messages = new List<object>();

        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            messages.Add(new
            {
                role = "system",
                content = request.SystemPrompt
            });
        }

        messages.AddRange(
            request.Messages.Select(message => new
            {
                role = message.Role switch
                {
                    AiMessageRole.System => "system",
                    AiMessageRole.User => "user",
                    AiMessageRole.Assistant => "assistant",
                    AiMessageRole.Tool => "tool",
                    _ => throw new ArgumentOutOfRangeException()
                },
                content = message.Content
            }));

        var payload = new
        {
            model = request.Model,
            messages,
            temperature = request.Temperature,
            max_tokens = request.MaxTokens,
            response_format = request.JsonMode
                ? new
                {
                    type = "json_object"
                }
                : null
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "chat/completions");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                _options.ApiKey);

        httpRequest.Content =
            JsonContent.Create(payload);

        using var response = await _httpClient.SendAsync(
            httpRequest,
            cancellationToken);

        var responseBody =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"AI provider returned {(int)response.StatusCode}: " +
                responseBody);
        }

        var result =
            JsonSerializer.Deserialize<OpenRouterChatResponse>(
                responseBody,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (result?.Choices is null ||
            result.Choices.Count == 0)
        {
            throw new InvalidOperationException(
                "AI provider returned no choices.");
        }

        var choice = result.Choices[0];

        return new AiChatResponse
        {
            Content = choice.Message?.Content ?? string.Empty,
            Model = request.Model,
            Provider = Name,
            Usage = result.Usage is null
                ? null
                : new AiUsage(
                    result.Usage.PromptTokens,
                    result.Usage.CompletionTokens,
                    result.Usage.TotalTokens)
        };
    }

    public async IAsyncEnumerable<AiStreamChunk> StreamChatAsync(
        AiChatRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var response = await ChatAsync(
            request,
            cancellationToken);

        yield return new AiStreamChunk
        {
            ContentDelta = response.Content,
            FinishReason = "stop",
            Usage = response.Usage
        };
    }
}