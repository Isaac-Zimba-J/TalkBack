using System;

namespace TalkBack.Models;

public class AudioRecording
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public string AudioFilePath { get; set; } = string.Empty;
    public string? TranscriptionText { get; set; }
    public bool IsTranscribed { get; set; }
    public long FileSizeBytes { get; set; }
}
