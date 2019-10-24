# PwshAudioExtraction
[![Minimum Supported PowerShell Version](https://img.shields.io/badge/PowerShell-6.1+-blue.svg)](https://github.com/PowerShell/PowerShell)
![Cross Platform](https://img.shields.io/badge/platform-windows%20%7C%20macos%20%7C%20linux-lightgrey)

A PowerShell module for ripping and tagging audio from various media sources.

- [Installation](#installation)
- [Functions](#functions)
- [License](#license)

## Dependencies

The following applications need to be installed on your machine:

- [youtube-dl](https://github.com/ytdl-org/youtube-dl/)
- [FFmpeg](https://github.com/FFmpeg/FFmpeg)

The following libraries are used in this module:

- [TagLibSharp](https://github.com/mono/taglib-sharp) (2.1.0) ([NuGet](https://www.nuget.org/packages/taglib/))

## Installation

We recommend you use Chocolatey to install youtube-dl and ffmpeg

## Functions

```powershell
Invoke-FetchAudio
Get-Id3Tag
New-SquareCroppedImage
Read-SourceAudio
Set-Id3Tag
Set-TrackArt
Update-Mp3Filename
```

## License

Distributed under the [MIT License](http://opensource.org/licenses/mit-license.php).
