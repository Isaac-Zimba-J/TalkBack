using System;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TalkBack.Models;

public partial class ActionItem : ObservableObject
{
    [ObservableProperty]
    [property: JsonPropertyName("task")]
    private string _task = string.Empty;

    [ObservableProperty]
    [property: JsonPropertyName("assignedTo")]
    private string _assignedTo = string.Empty;

    [ObservableProperty]
    [property: JsonPropertyName("dueDate")]
    private string _dueDate = string.Empty;

    [ObservableProperty]
    [property: JsonPropertyName("priority")]
    private string _priority = "medium";

    // Additional properties for app functionality
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private bool _isCompleted = false;

    [ObservableProperty]
    private DateTime? _reminderTime;

    [ObservableProperty]
    private string _recordingId = string.Empty;
}
