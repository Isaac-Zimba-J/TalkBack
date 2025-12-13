using System;
using System.Text.Json;
using TalkBack.Models;

namespace TalkBack.Services;

public class RecordingService
{
    private readonly string _recordingsDirectory;
    private readonly string _metadataFile;
    private List<AudioRecording> _recordings;

    public RecordingService()
    {
        _recordingsDirectory = Path.Combine(FileSystem.AppDataDirectory, "Recordings");
        _metadataFile = Path.Combine(FileSystem.AppDataDirectory, "recordings_metadata.json");
        _recordings = new List<AudioRecording>();

        // Ensure recordings directory exists
        Directory.CreateDirectory(_recordingsDirectory);

        // Load existing recordings
        LoadRecordings();
    }

    public async Task<AudioRecording> SaveRecordingAsync(Stream audioStream, TimeSpan duration)
    {
        var recording = new AudioRecording
        {
            RecordedAt = DateTime.Now,
            Duration = duration,
            Title = $"Recording {DateTime.Now:yyyy-MM-dd HH:mm}"
        };

        // Save audio file
        var fileName = $"{recording.Id}.wav";
        var filePath = Path.Combine(_recordingsDirectory, fileName);

        using (var fileStream = File.Create(filePath))
        {
            audioStream.Position = 0;
            await audioStream.CopyToAsync(fileStream);
        }

        recording.AudioFilePath = filePath;
        recording.FileSizeBytes = new FileInfo(filePath).Length;

        // Add to list and save metadata
        _recordings.Add(recording);
        await SaveMetadataAsync();

        return recording;
    }

    public async Task UpdateTranscriptionAsync(string recordingId, string transcription)
    {
        var recording = _recordings.FirstOrDefault(r => r.Id == recordingId);
        if (recording != null)
        {
            recording.TranscriptionText = transcription;
            recording.IsTranscribed = true;
            await SaveMetadataAsync();
        }
    }

    public async Task UpdateRecordingTitleAsync(string recordingId, string newTitle)
    {
        var recording = _recordings.FirstOrDefault(r => r.Id == recordingId);
        if (recording != null)
        {
            recording.Title = newTitle;
            await SaveMetadataAsync();
        }
    }

    public List<AudioRecording> GetAllRecordings()
    {
        return _recordings.OrderByDescending(r => r.RecordedAt).ToList();
    }

    public AudioRecording? GetRecording(string id)
    {
        return _recordings.FirstOrDefault(r => r.Id == id);
    }

    public async Task<bool> DeleteRecordingAsync(string id)
    {
        var recording = _recordings.FirstOrDefault(r => r.Id == id);
        if (recording != null)
        {
            // Delete audio file
            if (File.Exists(recording.AudioFilePath))
            {
                File.Delete(recording.AudioFilePath);
            }

            _recordings.Remove(recording);
            await SaveMetadataAsync();
            return true;
        }
        return false;
    }

    public Stream? GetAudioStream(string id)
    {
        var recording = GetRecording(id);
        if (recording != null && File.Exists(recording.AudioFilePath))
        {
            return File.OpenRead(recording.AudioFilePath);
        }
        return null;
    }

    private async Task SaveMetadataAsync()
    {
        var json = JsonSerializer.Serialize(_recordings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_metadataFile, json);
    }

    private void LoadRecordings()
    {
        if (File.Exists(_metadataFile))
        {
            try
            {
                var json = File.ReadAllText(_metadataFile);
                _recordings = JsonSerializer.Deserialize<List<AudioRecording>>(json) ?? new List<AudioRecording>();
            }
            catch
            {
                _recordings = new List<AudioRecording>();
            }
        }
    }
}