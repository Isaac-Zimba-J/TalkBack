# 🎙️ TalkBack – AI-Powered Meeting Transcription & Analysis App

![.NET MAUI](https://img.shields.io/badge/.NET-MAUI-512BD4)
![Azure AI](https://img.shields.io/badge/Azure-AI%20Foundry-0078D4)
![Whisper](https://img.shields.io/badge/AI-Whisper-orange)
![License](https://img.shields.io/badge/license-MIT-green)

TalkBack is a **cross-platform .NET MAUI mobile application** that records audio, transcribes it locally using **Whisper AI**, and analyzes meetings with **Azure AI (GPT-4)** to extract summaries, action items, assignees, priorities, and due dates.

---

## 🏗️ Architecture Diagram

```
┌─────────────┐
│  User UI    │
│ (MAUI/XAML) │
└──────┬──────┘
       ↓
┌─────────────┐
│ ViewModels  │  ← MVVM + Commands
└──────┬──────┘
       ↓
┌─────────────┐
│  Services   │
│ Audio / AI  │
└──────┬──────┘
       ↓
┌─────────────┐     ┌────────────────┐
│ Local Files │ ←→ │ Whisper (Local) │
└─────────────┘     └────────────────┘
       ↓
┌────────────────────┐
│ Azure AI (GPT-4)   │
└────────────────────┘
```

---

## 🚀 Features
- Audio recording (WAV)
- Local Whisper transcription
- Azure AI meeting analysis
- Action item extraction
- Cross-platform support

---

## 🧪 Testing

Testing focuses on **service and ViewModel layers**.

### Recommended Tests
- Unit tests for:
  - RecordingService
  - WhisperService
  - AzureWhisperService (mocked)
- ViewModel command execution
- JSON serialization/deserialization

### Tools
- xUnit / NUnit
- Moq / NSubstitute

---

## 📄 License
MIT License


# Contributing to TalkBack

Thank you for your interest in contributing! 🎉

## How to Contribute
1. Fork the repository
2. Create a feature branch
3. Write clean, well-documented code
4. Add or update tests
5. Submit a pull request

## Guidelines
- Follow MVVM principles
- Keep UI logic out of Views
- Use async/await correctly
- Write descriptive commit messages

## Code Style
- C# naming conventions
- XAML should remain clean and readable
- Prefer CommunityToolkit.Mvvm attributes


# TalkBack Architecture

## Overview
TalkBack follows a **layered MVVM architecture** optimized for testability and scalability.

## Layers
- **Views**: XAML UI
- **ViewModels**: State + Commands
- **Services**: Audio, AI, Persistence
- **Models**: Domain entities

## Key Design Decisions
- Local-first transcription for privacy
- Cloud AI only for analysis
- JSON-based persistence
- Dependency Injection for services

## AI Flow
1. Audio recorded
2. Whisper transcribes locally
3. Text sent to Azure AI
4. Structured JSON response parsed

## Scalability
- Services are singleton-based
- Easy to replace AI providers
- Platform-agnostic core logic
