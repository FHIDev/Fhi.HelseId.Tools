$exePath = "..\src\ClientSecretTool\Fhi.HelseId.ClientSecret.App\bin\Debug\net9.0\Fhi.HelseId.ClientSecret.App.exe"


$env:DOTNET_ENVIRONMENT = "Development"
& $exePath generatekey --FileName "name_ps" --KeyPath "C:\\temp"
Write-Host $output
   