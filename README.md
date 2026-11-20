# ZeroHour

[![License: MIT](https://img.shields.io/github/license/smokeluce/zerohour)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Platform: Windows](https://img.shields.io/badge/platform-windows-0078D4)](https://github.com/smokeluce/zerohour/releases)
[![Latest release](https://img.shields.io/github/v/release/smokeluce/zerohour)](https://github.com/smokeluce/zerohour/releases/latest)
[![Last commit](https://img.shields.io/github/last-commit/smokeluce/zerohour)](https://github.com/smokeluce/zerohour/commits/main)

A very colorful terminal countdown for Windows, built with .NET 10 and Spectre.Console. Rainbow-cycling big digits, an animated progress bar, and a celebration screen when the clock hits zero.

## Usage

Save an event with a numeric date code (`YYMMDDHHmm`, 24-hour time) and a name:

    zerohour --set 2612171300 --name "Released From Probation (probably)"

That is Dec 17 2026 at 1:00 PM. Then just run:

    zerohour

Change only the name later with `--name "New name"`, or only the date with `--set`. Press Ctrl+C to exit.

The event is saved as JSON at `%TEMP%\zerohour\event.json`, so it persists between runs. Windows cleanup tools can wipe the temp folder, in which case just run `--set` again.

## Build

    dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o dist

Add `dist` to your PATH and call `zerohour` from anywhere.

## License

MIT, see [LICENSE](LICENSE).

