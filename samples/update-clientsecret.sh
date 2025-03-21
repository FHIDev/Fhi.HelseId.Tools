#!/bin/bash

ClientId="37a08838-db82-xxx-xxxx-xxxxx"
NewKey="{\"alg\":\"RSA\",\"d\":\"xxx .....}"
OldKey="{\"alg\":\"PS512\",\"d\":\"xxx ....\"}"

export DOTNET_ENVIRONMENT=Development
 "..\src\ClientSecretTool\Fhi.HelseId.ClientSecret.App\bin\Debug\net9.0\Fhi.HelseId.ClientSecret.App.exe" updateclientkey --ClientId $ClientId --NewKey $NewKey --OldKey $OldKey  

