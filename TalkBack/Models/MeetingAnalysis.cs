using System;
using System.Text.Json.Serialization;

namespace TalkBack.Models;

public class MeetingAnalysis
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("actionItems")]
    public List<ActionItem> ActionItems { get; set; } = new();
}