#!/bin/bash

echo "Starting Script!"

echo "Installing tool..."

dotnet --version

#dotnet tool install --global Fhi.HelseIdSelvbetjening.CLI --version 1.0.0
dotnet tool install --global Fhi.HelseIdSelvbetjening.CLI --version 1.0.0-beta1

echo "Tool installed."

ClientId=""
KeyName="HelseIdJWK"
NewKeyDirectory="C:/NewHelseIDJWKKey"
CurrentKeyDirectory="C:/CurrentHelseIDJWKKey"
NewKeyPath="${KeyDirectory}/${KeyName}_public"
CurrentKeyPath="${CurrentKeyDirectory}/${KeyName}_private"
Authority="https://helseid-sts.test.nhn.no"
BaseAddress="https://api.selvbetjening.test.nhn.no"

echo "Generating key..."

helseid-cli generatejsonwebkey --KeyFileNamePrefix $ClientId --KeyDirectory $NewKeyDirectory

echo "Keys generated."

echo "Updating keys in HelseID...."

# helseid-cli updateclientkey --ClientId $ClientId --NewPublicJwkPath $NewKeyPath --ExistingPrivateJwkPath $CurrentKeyPath --Authority $Authority --BaseAddress $BaseAddress --yes

echo "Keys updated in HelseID."

echo "Deleting old keys..."

#rm $CurrentKeyPath

echo "Keys deleted."

echo "Moving new keys into currently used keys folder..."

#mv $NewKeyPath $CurrentKeyPath

echo "Keys moved."

echo "Job completed!"