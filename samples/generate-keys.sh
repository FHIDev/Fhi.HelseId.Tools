#!/bin/bash



export DOTNET_ENVIRONMENT=Development
 "..\src\Fhi.HelseIdSelvbetjening.CLI\bin\Debug\net9.0\Fhi.HelseIdSelvbetjening.CLI.exe" generatekey --FileName "37a08838-xxxx-xxx-xxxx-xxx" --KeyPath "C:\TestKeys"

