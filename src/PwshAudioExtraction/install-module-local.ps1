[CmdletBinding()]
Param()

$modulePaths = $Env:PSModulePath -split ';'
$localModulePath = $modulePaths -match [regex]::Escape("${env:UserProfile}\Documents\PowerShell\Modules")
$targetDir = Join-Path -Path $localModulePath -ChildPath "PwshAudioExtraction"

$folders = @(
  ".\Public"
  , ".\Private"
)

$files = @(
  "libSkiaSharp.*"
  , "PwshAudioExtraction.dll"
  , "PwshAudioExtraction.psd1"
  , "PwshAudioExtraction.psm1"
)

# Delete module folder
Get-ChildItem -Path $targetDir -Recurse | Remove-Item -Force -Recurse
Remove-Item $targetDir -Force -ErrorAction Continue

# Create new module folder, copy files
New-Item -ItemType Directory -Path $targetDir
$folders | Copy-Item -Destination $targetDir -Recurse
Get-ChildItem $files | Copy-Item -Destination $targetDir