using System;
using Plugin.Maui.Audio;

namespace TalkBack.Services;

public class AudioService
{
    private readonly IAudioManager _audioManager;
    private readonly IAudioRecorder _recorder;

    public AudioService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
        _recorder = _audioManager.CreateRecorder();
    }

    public Task StartRecordingAsync() =>
        _recorder.StartAsync();

    public async Task<Stream> StopRecordingAsync()
    {
        var audioSource = await _recorder.StopAsync();
        return audioSource.GetAudioStream();
    }
}
