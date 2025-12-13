# 🎙️ TalkBack – AI-Powered Meeting Transcription & Analysis App

TalkBack is a **cross-platform .NET MAUI mobile application** that records audio, transcribes it locally using **Whisper AI**, and analyzes meetings with **Azure AI (GPT-4)** to extract summaries, action items, assignees, priorities, and due dates.

This project demonstrates **enterprise-grade mobile development**, combining local AI, cloud AI, real-time audio processing, and modern MVVM patterns.

---

## 🚀 Features

- 🎤 **Audio Recording**
  - Record meetings directly from the app
  - WAV audio format stored locally
  - Metadata saved alongside recordings

- 📝 **Local Transcription (Offline-Friendly)**
  - Uses **Whisper.net** for on-device speech-to-text
  - Supports multiple Whisper model sizes (tiny → large)
  - Non-blocking async transcription

- 🧠 **AI Meeting Analysis**
  - Sends transcription to **Azure AI Foundry (GPT-4)**
  - Extracts:
    - Meeting summary
    - Action items
    - Assignees
    - Due dates
    - Priority levels

- 📋 **Action Item Management**
  - View extracted tasks
  - Priority badges
  - Clean, structured task UI

- 📱 **Cross-Platform**
  - Android
  - iOS
  - Windows (via .NET MAUI)

---

## 🏗️ Architecture Overview

### High-Level Flow

