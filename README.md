# WPF_Digital-Piano

 A digital piano desktop application built using WPF and .NET.

## 📑 Table of Contents

- [Description](#description)
- [Use Cases](#use-cases)
- [Tech Stack](#tech-stack)
- [Quick Start](#quick-start)
- [Available Scripts](#available-scripts)
- [Project Structure](#project-structure)
- [Development Setup](#development-setup)
- [Contributing](#contributing)

## 📝 Description

WPF_Digital-Piano is a Windows desktop application developed using Windows Presentation Foundation (WPF) and the .NET ecosystem to simulate a digital piano keyboard. The project addresses the need for a straightforward, local software-based musical instrument interface, giving users the ability to trigger notes directly on their Windows computers.

##  Use Cases

- Exploring WPF desktop application design and interactive event handling in C#.
- Developing and testing digital instrument simulations or basic audio synthesis in a Windows environment.

##  Tech Stack

-  .NET

##  Quick Start

```bash

# 1. Clone the repository
git clone https://github.com/nhatle1909/WPF_Digital-Piano.git

# Restore and run
dotnet restore && dotnet run
```

##  Available Scripts

- **run** — `dotnet run`
- **test** — `dotnet test`

##  Project Structure

```
.
├── TemplateEngineHost
│   └── vs
│       └── templatecache.json
├── WPF_Piano
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── AssemblyInfo.cs
│   ├── Behavior
│   │   └── ScrollViewerBehavior.cs
│   ├── CustomPianoButton.cs
│   ├── Deprecated
│   │   ├── NoteControl.xaml
│   │   └── NoteControl.xaml.cs
│   ├── Helper
│   │   ├── FrameworkElementHelper.cs
│   │   ├── OemStringMapper.cs
│   │   ├── PianoPlaySound.cs
│   │   ├── PianoSettings.cs
│   │   └── PianoUIRender.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── Model
│   │   ├── PianoButton.cs
│   │   ├── Song.cs
│   │   └── SongPlayer.cs
│   ├── NoteControl.cs
│   ├── Resources
│   │   ├── play.png
│   │   ├── settings.png
│   │   └── song.png
│   ├── SettingsWindow.xaml
│   ├── SettingsWindow.xaml.cs
│   ├── Song
│   │   ├── AnyConv.com__RADIANT_FORCE_off_vocal.mid
│   │   ├── Composed by Taniyama Hiroko_Singer_ Aoi Teshima_Arranged by WYNDERSYDE@YouTube_Transcripted by HaNguyen - Ai wo Komete Umi.mid
│   │   ├── Synchrogazer.mid
│   │   └── exterminate_ver2.mid
│   ├── StaticValue
│   │   └── NoteValue.cs
│   ├── Template.xaml
│   ├── ViewModel
│   │   ├── MainViewVM.cs
│   │   ├── PianoButtonVM.cs
│   │   ├── RelayCommand.cs
│   │   ├── SongPlayerVM.cs
│   │   └── SongVM.cs
│   ├── WPF_Piano.csproj
│   ├── WPF_Piano.csproj.user
│   └── appsettings.json
└── WPF_Piano.sln
```

## Development Setup

### .NET
1. Install the [.NET SDK](https://dotnet.microsoft.com/)
2. `dotnet restore && dotnet run`


