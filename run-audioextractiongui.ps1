<# This form was created using POSHGUI.com  a free online gui designer for PowerShell
.NAME
    downloader-gui
#>
[CmdletBinding()]
Param()

Add-Type -AssemblyName System.Windows.Forms
[System.Windows.Forms.Application]::EnableVisualStyles()

$Form = New-Object system.Windows.Forms.Form
$Form.ClientSize = '411,209'
$Form.text = "PwshAudioExtraction"
$Form.TopMost = $false

$CancelButton = New-Object system.Windows.Forms.Button
$CancelButton.text = "Cancel"
$CancelButton.width = 60
$CancelButton.height = 30
$CancelButton.location = New-Object System.Drawing.Point(166, 120)
$CancelButton.Font = 'Microsoft Sans Serif,10'

$ConfirmButton = New-Object system.Windows.Forms.Button
$ConfirmButton.text = "Download"
$ConfirmButton.width = 162
$ConfirmButton.height = 30
$ConfirmButton.location = New-Object System.Drawing.Point(238, 120)
$ConfirmButton.Font = 'Microsoft Sans Serif,10'

$OutputDirTextBox = New-Object system.Windows.Forms.TextBox
$OutputDirTextBox.multiline = $false
$OutputDirTextBox.width = 314
$OutputDirTextBox.height = 20
$OutputDirTextBox.enabled = $false
$OutputDirTextBox.location = New-Object System.Drawing.Point(15, 85)
$OutputDirTextBox.Font = 'Microsoft Sans Serif,10'

$OutputDirLabel = New-Object system.Windows.Forms.Label
$OutputDirLabel.text = "Location"
$OutputDirLabel.AutoSize = $true
$OutputDirLabel.width = 25
$OutputDirLabel.height = 10
$OutputDirLabel.location = New-Object System.Drawing.Point(15, 66)
$OutputDirLabel.Font = 'Microsoft Sans Serif,10'

$PickFolderButton = New-Object system.Windows.Forms.Button
$PickFolderButton.text = "..."
$PickFolderButton.width = 60
$PickFolderButton.height = 30
$PickFolderButton.location = New-Object System.Drawing.Point(340, 79)
$PickFolderButton.Font = 'Microsoft Sans Serif,10'

$SourceUrlTextBox = New-Object system.Windows.Forms.TextBox
$SourceUrlTextBox.multiline = $false
$SourceUrlTextBox.width = 314
$SourceUrlTextBox.height = 20
$SourceUrlTextBox.location = New-Object System.Drawing.Point(15, 36)
$SourceUrlTextBox.Font = 'Microsoft Sans Serif,10'

$SourceUrlLabel = New-Object system.Windows.Forms.Label
$SourceUrlLabel.text = "Source"
$SourceUrlLabel.AutoSize = $true
$SourceUrlLabel.width = 25
$SourceUrlLabel.height = 10
$SourceUrlLabel.location = New-Object System.Drawing.Point(15, 16)
$SourceUrlLabel.Font = 'Microsoft Sans Serif,10'

$SourceUrlResetButton = New-Object system.Windows.Forms.Button
$SourceUrlResetButton.text = "Reset"
$SourceUrlResetButton.width = 60
$SourceUrlResetButton.height = 30
$SourceUrlResetButton.location = New-Object System.Drawing.Point(340, 30)
$SourceUrlResetButton.Font = 'Microsoft Sans Serif,10'

$StatusBox = New-Object system.Windows.Forms.TextBox
$StatusBox.multiline = $false
$StatusBox.width = 384
$StatusBox.height = 20
$StatusBox.enabled = $false
$StatusBox.location = New-Object System.Drawing.Point(15, 169)
$StatusBox.Font = 'Microsoft Sans Serif,10'

$Form.controls.AddRange(@($CancelButton, $ConfirmButton, $OutputDirTextBox, $OutputDirLabel, $PickFolderButton, $SourceUrlTextBox, $SourceUrlLabel, $SourceUrlResetButton, $StatusBox))

$CancelButton.Add_Click( { CloseForm })
$PickFolderButton.Add_Click( { PickFolder })
$SourceUrlResetButton.Add_Click( { ResetSourceUrl })
$ConfirmButton.Add_Click( { InvokePwshAudioExtraction })

$Form.MinimizeBox = $False;
$Form.MaximizeBox = $False;
$Form.FormBorderStyle = [System.Windows.Forms.FormBorderStyle]::FixedSingle;
$OutputDirTextBox.text = [Environment]::GetFolderPath([System.Environment+SpecialFolder]::MyMusic)

if ([System.Windows.Forms.Clipboard]::ContainsText()) {
    $clipped = [System.Windows.Forms.Clipboard]::GetText()

    # https://www.regextester.com/96504
    $datePattern = [Regex]::new("(?:(?:https?|ftp):\/\/|\b(?:[a-z\d]+\.))(?:(?:[^\s()<>]+|\((?:[^\s()<>]+|(?:\([^\s()<>]+\)))?\))+(?:\((?:[^\s()<>]+|(?:\(?:[^\s()<>]+\)))?\)|[^\s`!()\[\]{};:'`".,<>?«»“”‘’]))?")
    $match = $datePattern.Match($clipped)
    if ($match.Success) {
        $SourceUrlTextBox.text = $match.Value
    }
    
}

function CloseForm() {
    $Form.close()
}

function PickFolder() {
    $FolderBrowser = New-Object System.Windows.Forms.FolderBrowserDialog
    $FolderBrowser.SelectedPath = $OutputDirTextBox.text
    
    if ($FolderBrowser.ShowDialog() -ne [System.Windows.Forms.DialogResult]::OK) {
        return
    }
    
    $OutputDirTextBox.text = $FolderBrowser.SelectedPath
}

function ResetSourceUrl() {
    $SourceUrlTextBox.text = $null
}

function InvokePwshAudioExtraction() {
    if ([string]::IsNullOrWhiteSpace($SourceUrlTextBox.text)) {
        return
    }
    
    $ConfirmButton.Enabled = $false
    
    # & pwsh.exe -NoProfile -NonInteractive -ExecutionPolicy ByPass -File E:\BuildAgent\temp\buildTmp\powershell5175709916209043362.ps1
    $command = "& { Import-Module PwshAudioExtraction; Invoke-DownloadAudio -Url `"$($SourceUrlTextBox.text)`" -OutputDir `"$($OutputDirTextBox.text)`" }"
    & pwsh.exe -NoProfile -NonInteractive -ExecutionPolicy ByPass -Command $command -Verbose
    $Form.close()

    Invoke-Item $OutputDirTextBox.text
}

[void]$Form.ShowDialog()