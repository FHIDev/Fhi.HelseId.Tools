#!/bin/bash

ClientId="37a08838-db82-xxx-xxxx-xxxxx"
NewKey="{\"alg\":\"RSA\",\"d\":\"xxx .....}"
OldKey="{\"alg\":\"PS512\",\"d\":\"xxx ....\"}"

export DOTNET_ENVIRONMENT=Development
 "..\src\Fhi.HelseIdSelvebetjening.CLI\bin\Debug\net9.0\Fhi.HelseIdSelvebetjening.CLI.exe"
 updateclientkey --ClientId $ClientId --NewKey $NewKey --OldKey $OldKey  

