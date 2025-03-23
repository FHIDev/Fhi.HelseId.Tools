$exePath = "..\src\Fhi.HelseIdSelvebetjening.CLI\bin\Debug\net9.0\Fhi.HelseIdSelvebetjening.CLI.exe"


$env:DOTNET_ENVIRONMENT = "Development"
& $exePath generatekey --FileName "name_ps" --KeyPath "C:\\temp"
Write-Host $output
   