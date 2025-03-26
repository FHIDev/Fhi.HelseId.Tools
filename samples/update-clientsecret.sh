#!/bin/bash

ClientId="37a08838-db82-xxx-xxxx-xxxxx"
NewKeyPath="C:\ClientSecretTest\37a08838-db82-4de0-bfe1-bed876e7086e_public.json"
ExisitingJwkPath="C:\ClientSecretTest\old_37a08838-db82-4de0-bfe1-bed876e7086e_private.json"

# Can use keys as argumenth or point to file where keys are stored
#NewKey="{\"alg\":\"PS512\",\"d\":\"xxx .....}"
#OldKey="{\"alg\":\"PS512\",\"d\":\"xxx ....\"}"

echo "ClientId: $ClientId"
echo "NewKeyPath: $NewKeyPath"
echo "ExisitingJwkPath: $ExisitingJwkPath"

export DOTNET_ENVIRONMENT=Test
 "..\src\Fhi.HelseIdSelvbetjening.CLI\bin\Debug\net9.0\Fhi.HelseIdSelvbetjening.CLI.exe" updateclientkey --ClientId $ClientId --NewPublicJwkPath $NewKeyPath --ExistingPrivateJwkPath $ExisitingJwkPath  

