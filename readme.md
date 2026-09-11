# Heads Up Game Clone

Our first app is a clone of the popular Heads Up game, which is a fun and interactive game where players guess words based on clues given by their teammates. The app will feature:

## Features
* Takes a video recording of the players during the game
* Can flip the phone up to pass or down to indicate a correct guess
* Uses AI to generate answers for the game based on categories
* AI also returns phonetic answers, so values like Belle and Bell are treated the same
* Uses Text to Speech to read out the answers
* Saves game history and scores using SQLite database
* Simple and intuitive user interface
* Cross-platform support for iOS and Android using .NET MAUI

## SETUP
* You need to supply your own Azure OpenAI URI & API Key in the Constants.cs file

## TODO
* Splash screen looks like junk
* FEATURE: AI Phonetic Answers and structured data
* BUG: Prevent android back button on ready/game screens
* BUG: MediaElement will turn off audio player sounds
* SensorAnswerDetector
  * BUG: Its more of a flip left/right 
* Try to use device AI instead of Azure OpenAI

This project was generated with the Shiny Templates
> dotnet new install Shiny.Templates

## Library Documentation

### .NET MAUI
_Microsoft Application User Interface Library_

* [Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
* [GitHub](https://github.com/dotnet/maui)

### Microsoft.Extensions.AI + Azure OpenAI
_Unified `IChatClient` abstraction for AI services. Backed by Azure OpenAI to generate category answers (with phonetic variants)._

* [Microsoft.Extensions.AI Documentation](https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai)
* [Azure OpenAI SDK Documentation](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/ai.openai-readme)
* [GitHub - Microsoft.Extensions.AI](https://github.com/dotnet/extensions)
* [GitHub - Azure SDK](https://github.com/Azure/azure-sdk-for-net)

### Community Toolkit MVVM
_A modern, fast, and modular MVVM library with source generators for observable properties and commands._

* [Documentation](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
* [GitHub](https://github.com/CommunityToolkit/dotnet)

### Shiny MAUI Shell
_Make .NET MAUI Shell shinier with viewmodel lifecycle management, source-generated routes, navigation, and more! - Written by Allan Ritchie_

* [Documentation](https://shinylib.net/mauishell/)
* [Dialogs (UXDivers integration)](https://shinylib.net/mauishell/dialogs/)
* [GitHub](https://github.com/shinyorg/mauishell)

### UXDivers Popups
_Popup controls library for .NET MAUI - used for dialogs via Shiny.Maui.Shell.UxDiversDialogs_

* [Documentation](https://uxdivers.com/popups)
* [GitHub](https://github.com/UXDivers/uxd-popups)

### Shiny Controls - MediaElement
_Cross-platform audio/video playback (AVPlayer, Media3/ExoPlayer) with a Shiny-drawn transport bar. Plays the recorded game video on the score screen._

* [Documentation](https://shinylib.net/controls/mediaelement/)
* [All Shiny Controls](https://shinylib.net/controls/)
* [GitHub](https://github.com/shinyorg/controls)

### Shiny Speech & Shiny Audio
_Cross-platform speech-to-text and text-to-speech (reads answers aloud, listens for spoken answers), plus audio playback for game sound effects_

* [Speech Documentation](https://shinylib.net/speech/)
* [Audio Documentation](https://shinylib.net/speech/audio/)
* [GitHub](https://github.com/shinyorg/speech)

### Shiny DocumentDb (SQLite)
_Document database for .NET with a SQLite provider - stores game history, scores, and categories. 100% AOT._

* [Documentation](https://shinylib.net/documentdb/)
* [GitHub](https://github.com/shinyorg/DocumentDb)

### Shiny Extensions
_Make .NET dependency injection less boilerplatey and add persistent service magic_

* [Dependency Injection](https://shinylib.net/di/) - `[Singleton]`/`[Transient]` source-generated registration
* [Stores](https://shinylib.net/stores/) - key/value stores & persistent services
* [MAUI Hosting](https://shinylib.net/foundation/hosting/maui/)
* [GitHub](https://github.com/shinyorg/extensions)

### Android Auto (AndroidX Car App)
_Car App Library bindings powering the Android Auto version of the game (`Platforms/Android/CarApp`)_

* [Documentation](https://developer.android.com/training/cars/apps)
* [GitHub - .NET Android bindings](https://github.com/dotnet/android-libraries)

### xUnit
_Unit testing framework used by GoneDotNet.HeadsUp.Tests_

* [Documentation](https://xunit.net/)
* [GitHub](https://github.com/xunit/xunit)

