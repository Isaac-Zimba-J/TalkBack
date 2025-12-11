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
            await _audio.StartRecordingAsync();
            return;
        }

        // Stop recording
        IsRecording = false;
        MicIcon = "blue_mic.png";
        TranscribedText = "Transcribing...";

        var audioStream = await _audio.StopRecordingAsync();
        var text = await _whisper.TranscribeAsync(audioStream);

        TranscribedText = text;
    }

}
