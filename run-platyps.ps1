# https://ntsystems.it/post/converting-powershell-help-a-website
# https://overpoweredshell.com/Module-Tools-Adding-Cmdlet-Help-With-PlatyPS/

Import-Module platyPS
Import-Module .\src\PwshAudioExtraction\PwshAudioExtraction.psd1

Remove-Item .\docs -Force -Recurse -ErrorAction SilentlyContinue
Remove-Item .\en-us -Force -Recurse -ErrorAction SilentlyContinue

$now = [DateTime]::Now.ToString("yyyy-MM-dd")

$meta = @{
  # 'layout'   = 'post';
  # 'author'   = 'thomas torggler';
  # 'title'    = $($cmdlet.Name);
  # 'category' = $($cmdlet.ModuleName).ToUpper();
  'date' = $now
}

$mdHelp = @{
  Module                = 'PwshAudioExtraction'
  OutputFolder          = '.\docs'
  AlphabeticParamsOrder = $true
  WithModulePage        = $true
  # Verbose               = $true
  Metadata              = $meta
}

New-MarkdownHelp @mdHelp
New-ExternalHelp -Path .\docs -OutputPath .\en-us