#!/bin/bash
echo "Starting Script!";
dotnet --version;
ClientId="$CLIENT_ID";
KeyName="HelseIdJWK";
NewKeyDirectory="/script/NewHelseIDJWKKey";
CurrentKeyDirectory="/script/CurrentHelseIDJWKKey";
NewKeyPath="${NewKeyDirectory}/${KeyName}_public";
CurrentKeyPath="${CurrentKeyDirectory}/${KeyName}_private";
Authority="https://helseid-sts.test.nhn.no";
BaseAddress="https://api.selvbetjening.test.nhn.no";
echo "Generating key...";
helseid-cli generatejsonwebkey --KeyFileNamePrefix $ClientId --KeyDirectory $NewKeyDirectory;
echo "Keys generated.";
echo "Updating keys in HelseID....";
# helseid-cli updateclientkey --ClientId $ClientId --NewPublicJwkPath $NewKeyPath --ExistingPrivateJwkPath $CurrentKeyPath --Authority $Authority --BaseAddress $BaseAddress --yes
echo "Keys updated in HelseID.";
echo "Update keys in Keyvault...";
# KEYVAULT
az --version;
echo "Keys updated in keyvault.";
echo "Job completed!";