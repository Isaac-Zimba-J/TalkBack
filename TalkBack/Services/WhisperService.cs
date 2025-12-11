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
        var processor = _factory.CreateBuilder()
            .WithLanguage("en")
            .Build();

        string result = "";

        await foreach (var segment in processor.ProcessAsync(audioStream))
            result += segment.Text;

        return result;
    }
}