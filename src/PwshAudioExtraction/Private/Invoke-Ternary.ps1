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
Function Invoke-Ternary {
  Param(
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