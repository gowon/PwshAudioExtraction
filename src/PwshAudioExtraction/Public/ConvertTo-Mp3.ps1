Function ConvertTo-Mp3 {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory = $false,
      ValueFromPipeline = $true,
      ValueFromPipelineByPropertyName = $true,
      Position = 0)]
    [ValidateNotNullOrEmpty()]
    [string[]]
    $Path = $null,

    [Parameter(Mandatory = $false,
      ValueFromPipeline = $true,
      ValueFromPipelineByPropertyName = $true,
      Position = 1)]
    [ValidateNotNullOrEmpty()]
    [string[]]
    $Include = $null
  )

  if ($null -eq $Path) {
    $Path = Get-Location
  }

  Get-ChildItem -Path $Path -Include $Include | ForEach-Object { 
    $base = Split-Path $_.Name -LeafBase
    Invoke-Expression -Command "ffmpeg -i `"$($_.FullName)`" -codec:a libmp3lame -qscale:a 0 `"$($base).mp3`""
  }
}