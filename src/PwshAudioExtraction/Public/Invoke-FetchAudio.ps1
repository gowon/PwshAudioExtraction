Function Invoke-FetchAudio {
  [CmdletBinding(DefaultParameterSetName = 'ImageSet')]
  Param(
    [Parameter(Mandatory = $true,
      ValueFromPipeline = $true,
      ValueFromPipelineByPropertyName = $true,
      Position = 0)]
    [ValidateNotNullOrEmpty()]
    [string]
    $Url,

    [Parameter(Mandatory = $false,
      Position = 1,
      ParameterSetName = 'ImageSet')]
    [ValidateNotNullOrEmpty()]
    [string]
    $Image,

    [Parameter(Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [string]
    $OutputDir,

    [Parameter(Mandatory = $false,
      ParameterSetName = 'UseFirstThumbnailSet')]
    [switch]
    $UseFirstThumbnail,

    [Parameter(Mandatory = $false,
      ParameterSetName = 'NoThumbnailSet')]
    [switch]
    $NoThumbnail
  )

  # resolve working directory
  $outputDirectory = Get-Location
  if (![string]::IsNullOrWhiteSpace($OutputDir)) {
    New-Item -ItemType Directory -Force -Path $OutputDir
    $outputDirectory = Resolve-Path -Path $OutputDir
  }

  Write-Verbose "Output directory is `"$($outputDirectory)`""
  Push-Location $outputDirectory -StackName "FetchAudio"

  # resolve image path
  $imagePath = $null
  $deleteDownloadedImage = $false
  if (![string]::IsNullOrWhiteSpace($Image)) {
    Write-Verbose "Using `"$($Image)`" as track art"
    if ([PwshAudio.Extensions.StringExtensions]::IsValidUrl($Image)) {
      Write-Verbose "URL detected, downloading image"
      $filename = $Image.Substring($Image.LastIndexOf("/") + 1)
      $imagePath = Join-Path -Path $outputDirectory -ChildPath $filename
      Invoke-WebRequest $Image -OutFile $imagePath
      $deleteDownloadedImage = $true
    }
    else {
      $imagePath = Resolve-Path -Path $Image
    }
  }

  $downloadCacheFilename = Join-Path -Path $outputDirectory -ChildPath "downloaded.txt"
  $writeThumbnail = ?: { -not $NoThumbnail } { "true" } { "false" }
  $json = "{`"FilesystemOptions`":{`"continueOpt`":true},`"GeneralOptions`":{`"ignoreErrors`":true},`"PostProcessingOptions`":{`"addMetadata`":true,`"audioFormat`":4,`"extractAudio`":true,`"postProcessorArgs`":`"-id3v2_version 3`"},`"ThumbnailImagesOptions`":{`"writeThumbnail`":$($writeThumbnail)},`"VideoFormatOptions`":{`"formatAdvanced`":`"bestaudio[ext=m4a]/bestaudio/best`"},`"VideoSelectionOptions`":{`"downloadArchive`":`"$($downloadCacheFilename)`"}}" -replace '\\', '\\'

  # Download file
  Read-SourceAudio -Url $Url -ConfigurationJson $json

  $ids = Get-Content $downloadCacheFilename | ForEach-Object { $_.Split()[1] }
  $ids | ForEach-Object {
    $mp3File = Get-ChildItem "*$($_)*" -Include *.mp3 | Select-Object -First 1
    Write-Verbose "Editing `"$($mp3File)`""

    # Set Track Art
    if (-not $NoThumbnail) {
      if ($null -ne $imagePath) {
        $imageFile = $imagePath
      }
      else {
        $id = $_
        if ($UseFirstThumbnail) {
          $id = $ids[0]
        }

        $imageFile = Get-ChildItem "*$($id)*" -Exclude *.mp3 | Select-Object -First 1
      }

      Write-Verbose "Using image `"$($imageFile)`""
      New-SquareCroppedImage $imageFile
      Set-TrackArt -Path $mp3File -ImagePath $imageFile
    }

    # Rename MP3
    Update-Mp3Filename $mp3File
  }

  # Cleanup
  $ids | ForEach-Object { Get-ChildItem "*$($_)*" -Exclude *.mp3 | Remove-Item }
  Remove-Item $downloadCacheFilename
  if ($deleteDownloadedImage) {
    Write-Verbose "Deleting `"$($imageFile)`""
    Remove-Item $imagePath -ErrorAction Continue
  }

  Pop-Location -StackName "FetchAudio"
}