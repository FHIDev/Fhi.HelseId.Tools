$exePath = "..\src\Fhi.HelseIdSelvbetjening.CLI\bin\Debug\net9.0\Fhi.HelseIdSelvbetjening.CLI.exe"
$clientId = ""
$newKey = ""
$oldKey =""


$env:DOTNET_ENVIRONMENT = "Test"

& $exePath updateclientkey --ClientId $clientId --NewClientJwk $newKey --OldClientJwk $oldKey

Write-Host $output

   