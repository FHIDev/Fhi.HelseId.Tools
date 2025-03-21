#!/bin/bash

ClientId="37a08838-db82-4de0-bfe1-bed876e7086e"
NewKey= "" 
OldKey=""


export DOTNET_ENVIRONMENT=Development
 "C:\Users\jhoug\source\repos\Fhi.HelseId.Tools\src\ClientSecretTool\Fhi.HelseId.ClientSecret.App\bin\Debug\net9.0\Fhi.HelseId.ClientSecret.App.exe" updateclientkey --ClientId $ClientId --NewKey $NewKey --OldKey ""  

