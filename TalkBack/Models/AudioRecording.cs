using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TalkBack.Models;

public partial class AudioRecording : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private DateTime _recordedAt;

    [ObservableProperty]
    private TimeSpan _duration;

    [ObservableProperty]
    private string _audioFilePath = string.Empty;

    [ObservableProperty]
    private string? _transcriptionText;

    [ObservableProperty]
    private bool _isTranscribed;

    [ObservableProperty]
    private long _fileSizeBytes;

    [ObservableProperty]
    private string? _meetingSummary;

    [ObservableProperty]
    private bool _isAnalyzed;

    [ObservableProperty]
    private List<ActionItem> _actionItems = new();
}
