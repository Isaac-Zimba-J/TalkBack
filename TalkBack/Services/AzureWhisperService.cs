using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure;
using Azure.AI.Inference;
using TalkBack.Models;

namespace TalkBack.Services;

public class AzureWhisperService
{
    private readonly ChatCompletionsClient _client;
    private readonly string _modelName = "gpt-4o";

    public AzureWhisperService(string endpoint, string apiKey)
    {
        var credential = new AzureKeyCredential(apiKey);
        // Azure AI Foundry requires /models endpoint
        var fullEndpoint = endpoint.TrimEnd('/') + "/models";
        _client = new ChatCompletionsClient(new Uri(fullEndpoint), credential);
    }

    public async Task<MeetingAnalysis> AnalyzeTranscriptionAsync(string transcriptionText)
    {
        try
        {
            var systemPrompt = @"You are an expert meeting assistant. Analyze the transcription and:
1. Create a structured meeting summary in minutes format with key discussion points
2. Extract ALL action items, especially when someone explicitly says 'action item' or implies a task
3. Identify any deadlines, time references, or dates for action items
4. Determine who is responsible for each action item if mentioned
5. Assess priority based on context (high/medium/low)

Format the response as JSON with this exact structure:
{
    ""summary"": ""Professional meeting summary in minutes format"",
    ""actionItems"": [
        {
            ""task"": ""Clear description of what needs to be done"",
            ""assignedTo"": ""Person's name if mentioned, otherwise empty string"",
            ""dueDate"": ""Extracted deadline in natural language (e.g., 'next Friday', 'by end of week', '2 days'), or empty string if none"",
            ""priority"": ""high or medium or low""
        }
    ]
}";

            var requestOptions = new ChatCompletionsOptions
            {
                Messages =
                {
                    new ChatRequestSystemMessage(systemPrompt),
                    new ChatRequestUserMessage($"Please analyze this meeting transcription and extract action items:\n\n{transcriptionText}")
                },
                Temperature = 0.3f,
                MaxTokens = 2000,
                Model = _modelName
            };

            var response = await _client.CompleteAsync(requestOptions);

            if (response?.Value?.Content == null)
            {
                throw new Exception("Received empty response from Azure AI");
            }

            var content = response.Value.Content;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var analysis = JsonSerializer.Deserialize<MeetingAnalysis>(content, options);
            return analysis ?? new MeetingAnalysis();
        }
        catch (Exception ex)
        {
            throw new Exception($"Azure AI analysis failed: {ex.Message}", ex);
        }
    }
}