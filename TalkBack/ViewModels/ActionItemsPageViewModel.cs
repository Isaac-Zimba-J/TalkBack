using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TalkBack.Models;
using TalkBack.Services;

namespace TalkBack.ViewModels;

public partial class ActionItemsPageViewModel : ObservableObject
{
    private readonly RecordingService _recordingService;

    [ObservableProperty]
    private ObservableCollection<ActionItem> _actionItems = new();

    [ObservableProperty]
    private ObservableCollection<ActionItem> _activeActionItems = new();

    [ObservableProperty]
    private ObservableCollection<ActionItem> _completedActionItems = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private int _activeCount;

    [ObservableProperty]
    private int _completedCount;

    [ObservableProperty]
    private bool _showCompleted = false;

    private List<ActionItem> _allActionItems = new();

    public ActionItemsPageViewModel(RecordingService recordingService)
    {
        _recordingService = recordingService;
    }

    public async Task LoadActionItemsAsync()
    {
        var recordings = _recordingService.GetAllRecordings();
        _allActionItems = recordings
            .Where(r => r.IsAnalyzed)
            .SelectMany(r => r.ActionItems)
            .OrderBy(a => a.IsCompleted)
            .ThenByDescending(a => a.Priority == "high" ? 3 : a.Priority == "medium" ? 2 : 1)
            .ToList();

        ApplyFilter();
    }

    [RelayCommand]
    private void ToggleCompleted()
    {
        ShowCompleted = !ShowCompleted;
    }

    [RelayCommand]
    private async Task ToggleActionItemComplete(ActionItem item)
    {
        item.IsCompleted = !item.IsCompleted;
        await _recordingService.UpdateActionItemAsync(item.RecordingId, item);
        await LoadActionItemsAsync();
    }

    [RelayCommand]
    private async Task DeleteActionItem(ActionItem item)
    {
        var confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Delete Action Item",
            $"Are you sure you want to delete '{item.Task}'?",
            "Delete",
            "Cancel");

        if (confirm)
        {
            var recording = _recordingService.GetRecording(item.RecordingId);
            if (recording != null)
            {
                recording.ActionItems.Remove(item);
                await _recordingService.UpdateMeetingAnalysisAsync(
                    recording.Id,
                    recording.MeetingSummary ?? string.Empty,
                    recording.ActionItems);
                await LoadActionItemsAsync();
            }
        }
    }

    [RelayCommand]
    private async Task ViewRecording(ActionItem item)
    {
        var recording = _recordingService.GetRecording(item.RecordingId);
        if (recording != null)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Recording Details",
                $"Title: {recording.Title}\n" +
                $"Date: {recording.RecordedAt:MMM dd, yyyy h:mm tt}\n" +
                $"Duration: {recording.Duration:mm\\:ss}",
                "OK");
        }
    }

    [RelayCommand]
    private async Task SetReminder(ActionItem item)
    {
        // For now, just show a message. You can implement actual reminders later
        await Application.Current!.MainPage!.DisplayAlert(
            "Set Reminder",
            "Reminder feature coming soon!",
            "OK");
    }

    partial void OnSearchTextChanged(string value)
    {
        IsSearching = !string.IsNullOrWhiteSpace(value);
        ApplyFilter();
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        IsSearching = false;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filtered = _allActionItems.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            filtered = filtered.Where(a =>
                a.Task.ToLower().Contains(searchLower) ||
                (a.AssignedTo?.ToLower().Contains(searchLower) ?? false));
        }

        var active = filtered.Where(a => !a.IsCompleted).ToList();
        var completed = filtered.Where(a => a.IsCompleted).ToList();

        ActiveActionItems = new ObservableCollection<ActionItem>(active);
        CompletedActionItems = new ObservableCollection<ActionItem>(completed);

        ActiveCount = active.Count;
        CompletedCount = completed.Count;

        ActionItems = ShowCompleted
            ? new ObservableCollection<ActionItem>(completed)
            : new ObservableCollection<ActionItem>(active);
    }
}
