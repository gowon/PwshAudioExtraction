# https://github.com/Pscx/Pscx/blob/master/Src/Pscx/Modules/Utility/Pscx.Utility.psm1
Set-Alias -Name ?: -Value Invoke-Ternary -Scope Script -Description "PSCX alias"

<#
.SYNOPSIS
    Similar to the C# ? : operator e.g. name = (value != null) ? String.Empty : value
.DESCRIPTION
    Similar to the C# ? : operator e.g. name = (value != null) ? String.Empty : value.
    The first script block is tested. If it evaluates to $true then the second scripblock
    is evaluated and its results are returned otherwise the third scriptblock is evaluated
    and its results are returned.
.PARAMETER Condition
    The condition that determines whether the TrueBlock scriptblock is used or the FalseBlock
    is used.
.PARAMETER TrueBlock
    This block gets evaluated and its contents are returned from the function if the Conditon
    scriptblock evaluates to $true.
.PARAMETER FalseBlock
    This block gets evaluated and its contents are returned from the function if the Conditon
    scriptblock evaluates to $false.
.PARAMETER InputObject
    Specifies the input object. Invoke-Ternary injects the InputObject into each scriptblock
    provided via the Condition, TrueBlock and FalseBlock parameters.
.EXAMPLE
    C:\PS> $toolPath = ?: {[IntPtr]::Size -eq 4} {"$env:ProgramFiles(x86)\Tools"} {"$env:ProgramFiles\Tools"}
    Each input number is evaluated to see if it is > 5.  If it is then "Greater than 5" is
    displayed otherwise "Less than or equal to 5" is displayed.
.EXAMPLE
    C:\PS> 1..10 | ?: {$_ -gt 5} {"Greater than 5";$_} {"Less than or equal to 5";$_}
    Each input number is evaluated to see if it is > 5.  If it is then "Greater than 5" is
    displayed otherwise "Less than or equal to 5" is displayed.
.NOTES
    Aliases:  ?:
    Author:   Karl Prosser
#>
function script:Invoke-Ternary {
	param(
		[Parameter(Mandatory, Position = 0)]
		[scriptblock]
		$Condition,

		[Parameter(Mandatory, Position = 1)]
		[scriptblock]
		$TrueBlock,

		[Parameter(Mandatory, Position = 2)]
		[scriptblock]
		$FalseBlock,

		[Parameter(ValueFromPipeline, ParameterSetName = 'InputObject')]
		[psobject]
		$InputObject
	)

	Process {
		if ($pscmdlet.ParameterSetName -eq 'InputObject') {
			Foreach-Object $Condition -input $InputObject | ForEach-Object {
				if ($_) {
					Foreach-Object $TrueBlock -InputObject $InputObject
				}
				else {
					Foreach-Object $FalseBlock -InputObject $InputObject
				}
			}
		}
		elseif (&$Condition) {
			&$TrueBlock
		}
		else {
			&$FalseBlock
		}
	}
}

function Invoke-FetchAudio {
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