---
external help file: PwshAudioExtraction.dll-help.xml
Module Name: PwshAudioExtraction
online version:
schema: 2.0.0
---

# Invoke-DownloadAudio

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

### ImageSet (Default)
```
Invoke-DownloadAudio [-Url] <String> [[-Image] <String>] [-OutputDir <String>] [<CommonParameters>]
```

### UseFirstThumbnailSet
```
Invoke-DownloadAudio [-Url] <String> [-OutputDir <String>] [-UseFirstThumbnail] [<CommonParameters>]
```

### NoThumbnailSet
```
Invoke-DownloadAudio [-Url] <String> [-OutputDir <String>] [-NoThumbnail] [<CommonParameters>]
```

## DESCRIPTION
{{ Fill in the Description }}

## EXAMPLES

### Example 1
```powershell
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -Image
{{ Fill Image Description }}

```yaml
Type: String
Parameter Sets: ImageSet
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoThumbnail
{{ Fill NoThumbnail Description }}

```yaml
Type: SwitchParameter
Parameter Sets: NoThumbnailSet
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputDir
{{ Fill OutputDir Description }}

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Url
{{ Fill Url Description }}

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -UseFirstThumbnail
{{ Fill UseFirstThumbnail Description }}

```yaml
Type: SwitchParameter
Parameter Sets: UseFirstThumbnailSet
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
