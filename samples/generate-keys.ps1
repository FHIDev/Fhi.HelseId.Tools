$exePath = "..\src\Fhi.HelseIdSelvbetjening.CLI\bin\Debug\net9.0\Fhi.HelseIdSelvbetjening.CLI.exe"


$env:DOTNET_ENVIRONMENT = "Development"
& $exePath generatekey --KeyFileNamePrefix "name_ps" --KeyDirectory "C:\\temp"
Write-Host $output
   