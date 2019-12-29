# PwshAudioExtraction

[![Minimum Supported PowerShell Version](https://img.shields.io/badge/PowerShell-6.1+-blue.svg)](https://github.com/PowerShell/PowerShell)
![Cross Platform](https://img.shields.io/badge/platform-windows%20%7C%20macos%20%7C%20linux-lightgrey)

A PowerShell module for ripping and tagging audio from various media sources.

## Dependencies

The following applications need to be installed on your machine:

- [youtube-dl](https://github.com/ytdl-org/youtube-dl/)
- [FFmpeg](https://github.com/FFmpeg/FFmpeg)

The following libraries are used in this module:

- [NYoutubeDL](https://gitlab.com/BrianAllred/NYoutubeDL)
- [SkiaSharp](https://github.com/mono/SkiaSharp)
- [TagLibSharp](https://github.com/mono/taglib-sharp)

## Installation

We recommend you use Chocolatey to install youtube-dl and ffmpeg.

## [Usage](docs)

The main functions from this module are:

- `ConvertTo-Mp3` - wraps over ffmpeg to convert video/audio files to MP3. Preserves metadata by default.
- `Invoke-DownloadAudio` - wraps over youtube-dl to rip audio from web sources.

## License

Distributed under the [MIT License](http://opensource.org/licenses/mit-license.php).
