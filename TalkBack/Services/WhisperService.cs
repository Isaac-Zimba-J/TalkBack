using System;
using Whisper.net;

namespace TalkBack.Services;

public class WhisperService
{
    private readonly WhisperFactory _factory;

    public WhisperService(string modelPath)
    {
        _factory = WhisperFactory.FromPath(modelPath);
    }

    public async Task<string> TranscribeAsync(Stream audioStream)
    {
        try
        {
            // Ensure stream is at the beginning
            if (audioStream.CanSeek)
                audioStream.Position = 0;

            var processor = _factory.CreateBuilder()
                .WithLanguage("en")
                .Build();

            string result = "";

            await foreach (var segment in processor.ProcessAsync(audioStream))
                result += segment.Text;

            return result;
        }
        catch (Exception ex)
        {
            // Add stream info to exception
            ex.Data["StreamLength"] = audioStream.Length;
            ex.Data["StreamPosition"] = audioStream.Position;
            ex.Data["CanSeek"] = audioStream.CanSeek;
            throw;
        }
    }
}