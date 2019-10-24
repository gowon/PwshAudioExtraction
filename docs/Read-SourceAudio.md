---
external help file: PwshAudioExtraction.dll-Help.xml
Module Name: PwshAudioExtraction
online version:
schema: 2.0.0
---

# Read-SourceAudio

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

### NoThumbnailParameterSet (Default)
```
Read-SourceAudio [-Url] <String> [-NoThumbnail] [<CommonParameters>]
```

### ConfigFileParameterSet
```
Read-SourceAudio [-Url] <String> [[-ConfigurationFile] <String>] [<CommonParameters>]
```

### ConfigJsonParameterSet
```
Read-SourceAudio [-Url] <String> [[-ConfigurationJson] <String>] [<CommonParameters>]
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

### -ConfigurationFile
{{ Fill ConfigurationFile Description }}

```yaml
Type: String
Parameter Sets: ConfigFileParameterSet
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -ConfigurationJson
{{ Fill ConfigurationJson Description }}

```yaml
Type: String
Parameter Sets: ConfigJsonParameterSet
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -NoThumbnail
{{ Fill NoThumbnail Description }}

```yaml
Type: SwitchParameter
Parameter Sets: NoThumbnailParameterSet
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
