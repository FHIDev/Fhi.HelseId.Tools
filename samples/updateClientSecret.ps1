$exePath = "..\src\ClientSecretTool\Fhi.HelseId.ClientSecret.App\bin\Debug\net9.0\Fhi.HelseId.ClientSecret.App.exe"
$clientId = "<clientId>"
$newKey = ""
$oldKey =""


$env:DOTNET_ENVIRONMENT = "Development"

& $exePath updateclientkey --ClientId $clientId --NewKey $newKey --OldKey $oldKey

Write-Host $output

   