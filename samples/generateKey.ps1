$exePath = "..\src\ClientSecretTool\Fhi.HelseId.ClientSecret.App\bin\Debug\net9.0\Fhi.HelseId.ClientSecret.App.exe"


$env:DOTNET_ENVIRONMENT = "Development"
& $exePath generatejsonwebkey --FileName "name" --KeyPath "C:\\temp"
Write-Host $output
   