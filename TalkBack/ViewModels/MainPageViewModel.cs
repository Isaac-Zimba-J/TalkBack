using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TalkBack.Services;

namespace TalkBack.ViewModels;

public partial class MainPageViewModel : ObservableObject
{

    private readonly AudioService _audio;
    private readonly WhisperService _whisper;


    [ObservableProperty]
    private string _transcribedText = "Tap the mic and speak...";

    [ObservableProperty]
    private bool _isRecording;
    [ObservableProperty]
    private string _micIcon = "blue_mic.png";

    private DateTime _recordingStartTime;


    public MainPageViewModel(AudioService audio, WhisperService whisper)
    {
        _audio = audio;
        _whisper = whisper;
    }

    [RelayCommand]
    public async Task ToggleRecording()
    {
        if (!IsRecording)
        {
            // Request microphone permission
            var status = await Permissions.RequestAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted)
            {
                TranscribedText = "Microphone permission denied";
                return;
            }

            IsRecording = true;
            MicIcon = "red_mic.png";
            TranscribedText = "Listening...";
            _recordingStartTime = DateTime.Now;
            await _audio.StartRecordingAsync();
            return;
        }

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
            return;
        }

        // Stop recording
        IsRecording = false;
        MicIcon = "blue_mic.png";
        TranscribedText = "Transcribing...";

        try
        {
            var audioStream = await _audio.StopRecordingAsync();

            if (audioStream == null || audioStream.Length == 0)
            {
                TranscribedText = "No audio recorded. Please try again.";
                audioStream?.Dispose();
                return;
            }

            var text = await _whisper.TranscribeAsync(audioStream);

            // Dispose the stream after transcription
            audioStream.Dispose();

            TranscribedText = string.IsNullOrWhiteSpace(text) ? "No speech detected" : text;
        }
        catch (Exception ex)
        {
            TranscribedText = $"Error: {ex.Message}";
        }
    }

}
