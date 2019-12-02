Function Convert-Audio {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory = $true,
      ValueFromPipeline = $true,
      ValueFromPipelineByPropertyName = $true,
      Position = 0)]
    [ValidateNotNullOrEmpty()]
    [string[]]
    $Path,

    [Parameter(Mandatory = $false)]
    [switch]
    $PreserveTags
  )

}