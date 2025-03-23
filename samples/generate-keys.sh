#!/bin/bash

ClientId="37a08838-db82-4de0-bfe1-bed876e7086e"
NewKey= "" 
OldKey=""


export DOTNET_ENVIRONMENT=Development
 "...\src\Fhi.HelseIdSelvebetjening.CLI\bin\Debug\net9.0\Fhi.HelseIdSelvebetjening.CLI.exe" updateclientkey --ClientId $ClientId --NewKey $NewKey --OldKey ""  

