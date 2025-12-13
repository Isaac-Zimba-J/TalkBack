using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Audio;
using TalkBack.Models;
using TalkBack.Services;
using Whisper.net;

namespace TalkBack.ViewModels;

public partial class RecordingsPageViewModel : ObservableObject
{

    private readonly RecordingService _recordingService;
    private readonly IAudioManager _audioManager;
    private readonly WhisperService _whisperService;
    private IAudioPlayer? _currentPlayer;

    [ObservableProperty]
    private ObservableCollection<AudioRecording> _recordings = new();
    private List<AudioRecording> _allRecordings = new();

    [ObservableProperty]
    private string? _currentlyPlayingId;

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    private string? _playIcon = "play.png";
    [ObservableProperty]
    private string? _currentlyTranscribingId;

    [ObservableProperty]
    private bool _isTranscribing;
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isSearching;

    private readonly AzureWhisperService _azureService;

    // Update constructor
    public RecordingsPageViewModel(
        RecordingService recordingService,
        IAudioManager audioManager,
        WhisperService whisperService,
        AzureWhisperService azureService)
    {
        _recordingService = recordingService;
        _audioManager = audioManager;
        _whisperService = whisperService;
        _azureService = azureService;
    }

    public async Task LoadRecordingsAsync()
    {
        var recordings = _recordingService.GetAllRecordings();
        _allRecordings = recordings.OrderByDescending(r => r.RecordedAt).ToList();
        Recordings = new ObservableCollection<AudioRecording>(_allRecordings);
    }


    [RelayCommand]
    private void SearchRecordings()
    {
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
        var filtered = _allRecordings.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            filtered = filtered.Where(r =>
                r.Title.ToLower().Contains(searchLower) ||
                (r.TranscriptionText?.ToLower().Contains(searchLower) ?? false));
        }

        Recordings = new ObservableCollection<AudioRecording>(
            filtered.OrderByDescending(r => r.RecordedAt));
    }

    partial void OnSearchTextChanged(string value)
    {
        IsSearching = !string.IsNullOrWhiteSpace(value);
        ApplyFilter();
    }

    [RelayCommand]
    private async Task EditRecordingTitle(AudioRecording recording)
    {
        var result = await Application.Current!.MainPage!.DisplayPromptAsync(
            "Edit Title",
            "Enter a new title for this recording:",
            initialValue: recording.Title,
            maxLength: 100,
            keyboard: Keyboard.Text);

        if (!string.IsNullOrWhiteSpace(result) && result != recording.Title)
        {
            await _recordingService.UpdateRecordingTitleAsync(recording.Id, result);
            recording.Title = result;

            // Note: No need to replace the object in Recordings since we're updating the property directly
            // The binding will automatically update the UI
        }
    }

    [RelayCommand]
    private async Task EditTranscription(AudioRecording recording)
    {
        if (!recording.IsTranscribed)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "No Transcription",
                "Please transcribe this recording first.",
                "OK");
            return;
        }

        var result = await Application.Current!.MainPage!.DisplayPromptAsync(
            "Edit Transcription",
            "Edit the transcription text:",
            initialValue: recording.TranscriptionText ?? "",
            maxLength: 5000,
            keyboard: Keyboard.Text);

        if (result != null && result != recording.TranscriptionText)
        {
            await _recordingService.UpdateTranscriptionAsync(recording.Id, result);
            recording.TranscriptionText = result;

            // Note: No need to replace the object in Recordings since we're updating the property directly
            // The binding will automatically update the UI
        }
    }


    [RelayCommand]
    private async Task PlayRecording(AudioRecording recording)
    {
        try
        {
            // If clicking the same recording that's currently loaded
            if (CurrentlyPlayingId == recording.Id && _currentPlayer != null)
            {
                // Toggle play/pause
                if (_currentPlayer.IsPlaying)
                {
                    _currentPlayer.Pause();
                    IsPlaying = false;
                }
                else
                {
                    _currentPlayer.Play();
                    IsPlaying = true;
                }
                return;
            }

            // Stop and dispose previous player if exists
            if (_currentPlayer != null)
            {
                _currentPlayer.Stop();
                _currentPlayer.Dispose();
                _currentPlayer = null;
            }

            // Start new playback
            var audioStream = _recordingService.GetAudioStream(recording.Id);
            if (audioStream == null)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", "Audio file not found", "OK");
                return;
            }

            _currentPlayer = _audioManager.CreatePlayer(audioStream);
            CurrentlyPlayingId = recording.Id;

            _currentPlayer.PlaybackEnded += (sender, args) =>
            {
                CurrentlyPlayingId = null;
                IsPlaying = false;
                _currentPlayer?.Dispose();
                _currentPlayer = null;
            };

            _currentPlayer.Play();
            IsPlaying = true;
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to play recording: {ex.Message}", "OK");
        }
    }


    [RelayCommand]
    private async Task TranscribeRecording(AudioRecording recording)
    {
        if (recording.IsTranscribed)
        {
            // show existing transcription
            await Application.Current.MainPage!.DisplayAlertAsync("Transcription", recording.TranscriptionText ?? "No transcription available.", "OK");
            return;
        }

        try
        {
            IsTranscribing = true;
            CurrentlyTranscribingId = recording.Id;

            var audioStream = _recordingService.GetAudioStream(recording.Id);
            if (audioStream == null)
            {
                CurrentlyTranscribingId = null;
                IsTranscribing = false;
                await Application.Current!.MainPage!.DisplayAlert("Error", "Audio file not found", "OK");
                return;
            }

            // transcribe the audio 
            var transcription = await _whisperService.TranscribeAsync(audioStream);
            audioStream.Dispose();

            // update the recording with transcription
            await _recordingService.UpdateTranscriptionAsync(recording.Id, transcription);

            //update the ui
            recording.TranscriptionText = transcription;
            recording.IsTranscribed = true;

            CurrentlyTranscribingId = null;
            IsTranscribing = false;

            // Note: No need to replace the object in Recordings since we're updating the properties directly
            // The binding will automatically update the UI

            await Application.Current!.MainPage!.DisplayAlert(
                   "Success",
                   "Transcription completed!",
                   "OK");
        }
        catch (Exception ex)
        {
            CurrentlyTranscribingId = null;
            IsTranscribing = false;
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to transcribe: {ex.Message}", "OK");
        }
    }


    [RelayCommand]
    private async Task AnalyzeMeeting(AudioRecording recording)
    {
        if (!recording.IsTranscribed)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Not Transcribed",
                "Please transcribe this recording first before analyzing.",
                "OK");
            return;
        }

        if (recording.IsAnalyzed)
        {
            // Show existing analysis
            await ShowMeetingAnalysis(recording);
            return;
        }

        try
        {
            IsTranscribing = true; // Reuse the loading indicator

            var analysis = await _azureService.AnalyzeTranscriptionAsync(recording.TranscriptionText!);

            recording.MeetingSummary = analysis.Summary;
            recording.ActionItems = analysis.ActionItems;
            foreach (var item in recording.ActionItems)
            {
                item.RecordingId = recording.Id;
            }
            recording.IsAnalyzed = true;

            // Save to database
            await _recordingService.UpdateMeetingAnalysisAsync(
                recording.Id,
                analysis.Summary,
                analysis.ActionItems);

            IsTranscribing = false;

            await ShowMeetingAnalysis(recording);
        }
        catch (Exception ex)
        {
            IsTranscribing = false;
            await Application.Current!.MainPage!.DisplayAlert(
                "Error",
                $"Failed to analyze meeting: {ex.Message}",
                "OK");
        }
    }


    private async Task ShowMeetingAnalysis(AudioRecording recording)
    {
        // Show summary first, then navigate to action items
        var actionItemsSummary = recording.ActionItems.Count > 0
            ? $"\n\nFound {recording.ActionItems.Count} action items."
            : "\n\nNo action items found.";

        var viewItems = recording.ActionItems.Count > 0
            ? await Application.Current!.MainPage!.DisplayAlert(
                "Meeting Analysis Complete",
                $"{recording.MeetingSummary}{actionItemsSummary}",
                "View Action Items",
                "Close")
            : false;

        if (viewItems)
        {
            await Shell.Current.GoToAsync("//ActionItemsPage");
        }
    }


    [RelayCommand]
    private async Task DeleteRecording(AudioRecording recording)
    {
        var confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Delete Recording",
            $"Are you sure you want to delete '{recording.Title}'?",
            "Delete",
            "Cancel");

        if (confirm)
        {
            // Stop playback if deleting current playing
            if (CurrentlyPlayingId == recording.Id)
            {
                _currentPlayer?.Stop();
                _currentPlayer?.Dispose();
                _currentPlayer = null;
                CurrentlyPlayingId = null;
                IsPlaying = false;
            }

            await _recordingService.DeleteRecordingAsync(recording.Id);
            Recordings.Remove(recording);
        }
    }

    public void OnDisappearing()
    {
        _currentPlayer?.Stop();
        _currentPlayer?.Dispose();
        _currentPlayer = null;
        CurrentlyPlayingId = null;
        IsPlaying = false;
    }
}