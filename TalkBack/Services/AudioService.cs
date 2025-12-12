using System;
using System.Text;
using Plugin.Maui.Audio;

namespace TalkBack.Services;

public class AudioService
{
    private readonly IAudioManager _audioManager;
    private IAudioStreamer? _streamer;
    private List<byte> _audioData = new();
    private DateTime _recordingStartTime;

    public AudioService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    public TimeSpan GetRecordingDuration()
    {
        return DateTime.Now - _recordingStartTime;
    }

    public async Task StartRecordingAsync()
    {
        // Clear previous recording data
        _audioData.Clear();
        _recordingStartTime = DateTime.Now;

        // Create a new streamer for raw PCM audio capture
        _streamer = _audioManager.CreateStreamer();

        // Configure for Whisper's requirements: 16kHz, mono, 16-bit
        _streamer.Options.SampleRate = 16000;
        _streamer.Options.Channels = ChannelType.Mono;
        _streamer.Options.BitDepth = BitDepth.Pcm16bit;

        // Subscribe to audio data events
        _streamer.OnAudioCaptured += OnAudioDataCaptured;

        await _streamer.StartAsync();
    }

    private void OnAudioDataCaptured(object? sender, AudioStreamEventArgs e)
    {
        // Collect raw PCM audio data
        _audioData.AddRange(e.Audio);
    }

    public async Task<Stream> StopRecordingAsync()
    {
        if (_streamer == null)
            throw new InvalidOperationException("No active recording");

        await _streamer.StopAsync();
        _streamer.OnAudioCaptured -= OnAudioDataCaptured;

        if (_audioData.Count < 1000)
            throw new InvalidOperationException($"Recording too small: {_audioData.Count} bytes. Please speak for at least 2-3 seconds.");

        // Convert raw PCM to WAV format with proper headers
        var wavData = ConvertPcmToWav(_audioData.ToArray(), 16000, 1, 16);

        return new MemoryStream(wavData);
    }

    private byte[] ConvertPcmToWav(byte[] pcmData, int sampleRate, int channels, int bitsPerSample)
    {
        using var memStream = new MemoryStream();
        using var writer = new BinaryWriter(memStream, System.Text.Encoding.UTF8);

        // WAV file header
        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + pcmData.Length); // File size - 8
        writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

        // Format chunk
        writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16); // Chunk size
        writer.Write((short)1); // Audio format (1 = PCM)
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * bitsPerSample / 8); // Byte rate
        writer.Write((short)(channels * bitsPerSample / 8)); // Block align
        writer.Write((short)bitsPerSample);

        // Data chunk
        writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        writer.Write(pcmData.Length);
        writer.Write(pcmData);

        return memStream.ToArray();
    }
}
