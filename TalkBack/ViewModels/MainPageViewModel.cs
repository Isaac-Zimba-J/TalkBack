using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TalkBack.Services;

namespace TalkBack.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly AudioService _audio;
    private readonly WhisperService _whisper;
    private readonly RecordingService _recordingService;
    private DateTime _recordingStartTime;
    private System.Timers.Timer? _recordingTimer;
    private const int MaxRecordingSeconds = 30 * 60; // 30 minutes

    [ObservableProperty]
    private string _transcribedText = "Tap the mic and speak...";

    [ObservableProperty]
    private bool _isRecording;

    [ObservableProperty]
    private string _micIcon = "blue_mic.png";

    [ObservableProperty]
    private string _recordingTime = "00:00";

    [ObservableProperty]
    private double _recordingProgress = 0;

    [ObservableProperty]
    private bool _showTimer;

    [ObservableProperty]
    private bool _autoTranscribe = false; // Off by default for Phase 4

    public MainPageViewModel(AudioService audio, WhisperService whisper, RecordingService recordingService)
    {
        _audio = audio;
        _whisper = whisper;
        _recordingService = recordingService;
    }


    [RelayCommand]
    private async Task ToggleRecording()
    {
        if (!IsRecording)
        {
            // request microphone permission
            var status = await Permissions.RequestAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted)
            {
                await App.Current.MainPage.DisplayAlert("Permission Denied", "Microphone permission is required to record audio.", "OK");
                return;
            }

            IsRecording = true;
            MicIcon = "red_mic.png";
            TranscribedText = "Recording... Tap the mic to stop.";
            ShowTimer = true;
            _recordingStartTime = DateTime.Now;

            // Start the timer
            StartRecordingTimer();

            await _audio.StartRecordingAsync();
            return;
        }

        // Stop recording manually
        await StopRecordingAsync();
    }



    private void StartRecordingTimer()
    {
        _recordingTimer = new System.Timers.Timer(100); // Update every 100ms
        _recordingTimer.Elapsed += (sender, e) =>
        {
            var elapsed = DateTime.Now - _recordingStartTime;
            var totalSeconds = elapsed.TotalSeconds;

            // Update UI on main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                RecordingTime = $"{(int)elapsed.TotalMinutes:D2}:{elapsed.Seconds:D2}";
                RecordingProgress = totalSeconds / MaxRecordingSeconds;

                // Auto-stop at max time
                if (totalSeconds >= MaxRecordingSeconds)
                {
                    _ = StopRecordingAsync();
                }
            });
        };
        _recordingTimer.Start();
    }

    private void StopRecordingTimer()
    {
        if (_recordingTimer != null)
        {
            _recordingTimer.Stop();
            _recordingTimer.Dispose();
            _recordingTimer = null;
        }
    }

    private async Task StopRecordingAsync()
    {
        if (!IsRecording)
            return;

        // Stop timer
        StopRecordingTimer();

        // Check minimum recording duration
        var recordingDuration = DateTime.Now - _recordingStartTime;
        if (recordingDuration.TotalSeconds < 2)
        {
            TranscribedText = "Please record for at least 2 seconds";
            try
            {
                await _audio.StopRecordingAsync();
            }
            catch { }
            IsRecording = false;
            MicIcon = "blue_mic.png";
            ShowTimer = false;
            RecordingTime = "00:00";
            RecordingProgress = 0;
            return;
        }
        // Stop recording
        IsRecording = false;
        MicIcon = "blue_mic.png";
        ShowTimer = false;
        TranscribedText = "Processing...";

        try
        {
            var audioStream = await _audio.StopRecordingAsync();

            if (audioStream == null || audioStream.Length == 0)
            {
                TranscribedText = "No audio recorded. Please try again.";
                audioStream?.Dispose();
                RecordingTime = "00:00";
                RecordingProgress = 0;
                return;
            }

            // Save the recording first
            var duration = _audio.GetRecordingDuration();
            var recording = await _recordingService.SaveRecordingAsync(audioStream, duration);

            // Only transcribe if auto-transcribe is enabled
            if (AutoTranscribe)
            {
                TranscribedText = "Recording saved! Transcribing...";

                // Reset stream for transcription
                audioStream.Position = 0;
                var text = await _whisper.TranscribeAsync(audioStream);

                // Save transcription
                await _recordingService.UpdateTranscriptionAsync(recording.Id, text);

                TranscribedText = string.IsNullOrWhiteSpace(text)
                    ? "Recording saved. No speech detected."
                    : $"Recording saved!\n\n{text}";
            }
            else
            {
                TranscribedText = "Recording saved! Go to Recordings tab to transcribe.";
            }

            // Dispose the stream
            audioStream.Dispose();

            // Reset timer display
            RecordingTime = "00:00";
            RecordingProgress = 0;
        }
        catch (Exception ex)
        {
            TranscribedText = $"Error: {ex.Message}";
            RecordingTime = "00:00";
            RecordingProgress = 0;
        }
    }
}