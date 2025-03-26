#!/bin/bash

# To set the HelseID selvbetjening environment
#export DOTNET_ENVIRONMENT=Production
export DOTNET_ENVIRONMENT=Test
 "..\src\Fhi.HelseIdSelvbetjening.CLI\bin\Debug\net9.0\Fhi.HelseIdSelvbetjening.CLI.exe" generatekey --KeyFileNamePrefix "37a08838-xxxx-xxx-xxxx-xxx" --KeyDirectory "C:\TestKeys"

