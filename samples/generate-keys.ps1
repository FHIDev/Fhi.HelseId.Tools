dotnet tool install --global Fhi.HelseIdSelvbetjening.CLI --version 1.0.0-beta1

$env:DOTNET_ENVIRONMENT = "Test"
& helseid-cli generatekey --KeyFileNamePrefix "prefixname" --KeyDirectory "C:\\temp"
Write-Host $output
   