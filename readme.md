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

## TODO
* FEATURE: AI Phonetic Answers and structured data
* BUG: Prevent android back button on ready/game screens
* BUG: MediaElement will turn off audio player sounds
* SensorAnswerDetector
  * BUG: Its more of a flip left/right 
* Score Screen
  * BUG: Video not auto playing and no transparency through score

This project was generated with the Shiny Templates
> dotnet new install Shiny.Templates

## Library Documentation

### .NET MAUI
_Microsoft Application User Interface Library_

* [Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
* [GitHub](https://github.com/dotnet/maui)


### Shiny Extensions

_A collection of extensions to the Shiny framework that provide additional functionality and services. These extensions are designed to enhance the capabilities of dependency injection, reflection, and application state._

* [Dependency Injection](https://shinylib.net/extensions/di/)
* [App Stores](https://shinylib.net/extensions/stores/)
* [Reflector](https://shinylib.net/extensions/reflector/) - Reflection Source Generator - NOT installed by default

### Shiny MAUI Shell
_Make .NET MAUI Shell shinier with viewmodel lifecycle management, navigation, and more! - Written by Allan Ritchie_

* [Documentation](https://shinylib.net/)
* [GitHub](https://github.com/shinyorg/shiny)

### Community Toolkit MVVM

The CommunityToolkit.Mvvm package (aka MVVM Toolkit, formerly named Microsoft.Toolkit.Mvvm) is a modern, fast, and modular MVVM library. It is part of the .NET Community Toolkit and is built around the following principles:

Platform and Runtime Independent - .NET Standard 2.0, .NET Standard 2.1 and .NET 6 🚀 (UI Framework Agnostic)
Simple to pick-up and use - No strict requirements on Application structure or coding-paradigms (outside of 'MVVM'ness), i.e., flexible usage.
À la carte - Freedom to choose which components to use.
Reference Implementation - Lean and performant, providing implementations for interfaces that are included in the Base Class Library, but lack concrete types to use them directly.

* [Documentation](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
* [GitHub](https://github.com/CommunityToolkit/dotnet)


### MAUI Community Toolkit

_A collection of reusable elements for application development with .NET MAUI, including animations, behaviors, converters, effects, and helpers. It simplifies and demonstrates common developer tasks when building iOS, Android, macOS and WinUI applications._

* [Documentation](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/)
* [GitHub](https://github.com/CommunityToolkit/Maui)

### MAUI Community Toolkit - Media Element

MediaElement is a view for playing video and audio in your .NET MAUI app.

* [Documentation](https://learn.microsoft.com/en-ca/dotnet/communitytoolkit/maui/views/mediaelement)
* [GitHub](https://github.com/CommunityToolkit/Maui)

## MAUI Audio Plugin

_Provides the ability to play audio inside a .NET MAUI application. - Written by Gerald Versluis_

* [GitHub](https://github.com/jfversluis/Plugin.Maui.Audio)

### SQLite .NET PCL

_SQLite-net is an open source, minimal library to allow .NET, .NET Core, and Mono applications to store data in SQLite 3 databases - Written by Frank Krueger_

[GitHub](https://github.com/praeclarum/sqlite-net)

