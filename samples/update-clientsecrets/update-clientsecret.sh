#!/bin/bash

CLIENTID=""
API_URL=""
KEY_PAPTH=""
OLD_KEY=""

dotnet run generate --clientId $CLIENTID --key-path $KEY_PAPTH
dotnet run update --clientId $CLIENTID --public-key-path $KEY_PAPTH --old-privatekey  $OLD_KEY --api-url $API_URL

