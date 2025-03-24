#!/bin/bash

ClientId="37a08838-db82-xxx-xxxx-xxxxx"
NewClientJwkPath = "C:\\TestKeys\\37a08838-db82-4de0-bfe1-bed876e7086e_public.json"
OldClientJwkPath = "C:\\TestKeys\\old_37a08838-db82-4de0-bfe1-bed876e7086e_private.json"

# Can use keys as argumenth or point to file where keys are stored
#NewKey="{\"alg\":\"PS512\",\"d\":\"xxx .....}"
#OldKey="{\"alg\":\"PS512\",\"d\":\"xxx ....\"}"

export DOTNET_ENVIRONMENT=Development
 "..\src\Fhi.HelseIdSelvbetjening.CLI\bin\Debug\net9.0\Fhi.HelseIdSelvbetjening.CLI.exe" updateclientkey --ClientId $clientId --NewClientJwkPath NewClientJwkPath --OldClientJwkPath $OldClientJwkPath  

