#!/bin/bash
echo "Starting Script!";
# Ensures that dotnet is properly installed
dotnet --version;
ClientId="$CLIENT_ID";
KeyName="HelseIdJWK";
NewKeyDirectory="/script/NewHelseIDJWKKey";
NewKeyPath="${NewKeyDirectory}/${KeyName}_public";
CurrentPublicKey="$PUBLIC_KEY";
Authority="https://helseid-sts.test.nhn.no";
BaseAddress="https://api.selvbetjening.test.nhn.no";
echo "Generating key...";
helseid-cli generatejsonwebkey --KeyFileNamePrefix $ClientId --KeyDirectory $NewKeyDirectory;
echo "Keys generated.";
echo "Updating keys in HelseID....";
# helseid-cli updateclientkey --ClientId $ClientId --NewPublicJwkPath $NewKeyPath --ExistingPrivateJwkPath $CurrentPublicKey --Authority $Authority --BaseAddress $BaseAddress --yes
echo "Keys updated in HelseID.";
echo "Update keys in Keyvault...";
# Ensures that azure is properly installed
az --version;
# We need to send/ update the generated keys in this script to keyvault before closing
# Once they keys have been updated in Keyvault, Skybert will be able to use the new keys the next time it runs this script
echo "Keys updated in keyvault.";
echo "Job completed!";