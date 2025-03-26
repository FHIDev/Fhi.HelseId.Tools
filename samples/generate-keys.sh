#!/bin/bash

# To set the HelseID selvbetjening environment
#export DOTNET_ENVIRONMENT=Production
export DOTNET_ENVIRONMENT=Test
 "..\src\Fhi.HelseIdSelvbetjening.CLI\bin\Debug\net9.0\Fhi.HelseIdSelvbetjening.CLI.exe" generatekey --FileName "37a08838-xxxx-xxx-xxxx-xxx" --KeyPath "C:\TestKeys"

