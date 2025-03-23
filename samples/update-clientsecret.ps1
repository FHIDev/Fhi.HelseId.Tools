$exePath = "..\src\Fhi.HelseIdSelvebetjening.CLI\bin\Debug\net9.0\Fhi.HelseIdSelvebetjening.CLI.exe"
$clientId = "<clientId>"
$newKey = ""
$oldKey =""


$env:DOTNET_ENVIRONMENT = "Development"

& $exePath updateclientkey --ClientId $clientId --NewKey $newKey --OldKey $oldKey

Write-Host $output

   