using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Audio;
using TalkBack.Models;
using TalkBack.Services;

namespace TalkBack.ViewModels;

public partial class RecordingsPageViewModel : ObservableObject
{

    private readonly RecordingService _recordingService;
    private readonly IAudioManager _audioManager;
    private IAudioPlayer? _currentPlayer;

    [ObservableProperty]
    private ObservableCollection<AudioRecording> _recordings = new();

    [ObservableProperty]
    private string? _currentlyPlayingId;

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    private string? _playIcon = "play.png";

    public RecordingsPageViewModel(RecordingService recordingService, IAudioManager audioManager)
    {
        _recordingService = recordingService;
        _audioManager = audioManager;
    }

    public async Task LoadRecordingsAsync()
    {
        var recordings = _recordingService.GetAllRecordings();
        Recordings = new ObservableCollection<AudioRecording>(recordings.OrderByDescending(r => r.RecordedAt));
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