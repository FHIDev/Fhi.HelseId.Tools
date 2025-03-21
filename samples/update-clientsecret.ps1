$exePath = "C:\Users\jhoug\source\repos\Fhi.HelseId.Tools\src\ClientSecretTool\Fhi.HelseId.ClientSecret.App\bin\Debug\net9.0\Fhi.HelseId.ClientSecret.App.exe"
$clientId = "<clientId>"
$newKey = ""
$oldKey = @'
{
	"alg": "PS512",
	"d": "<>",
	"dp": "<>",
	"dq": "<>",
	"e": "<>",
	"kid": "<>",
	"kty": "RSA",
	"n": "<>",
	"p": "<>",
	"q": "<>",
	"qi": "<>"
}
'@


$env:DOTNET_ENVIRONMENT = "Development"

& $exePath updateclientkey --ClientId $clientId --NewKey $newKey --OldKey $oldKey

Write-Host $output
echo y
   