# Generate new Json Web keys Commands

## Parameters

|Parameter name | Descrition																					| Required | Sample						|
|---------------|-----------------------------------------------------------------------------------------------|----------|-------------------------------|
|KeyFileNamePrefix| Prefix of name of the public and private key file. <br> The keys will be named `<FileName>_private.json` and `<FileName>_public.json`.|<b>Yes</b>|`"newKey"`|
|KeyDirectory| Path to where private and public key will be stored.|<b>Yes</b>|`"C:\\temp"`|

## Commands
```
 generatejsonwebkey --KeyFileNamePrefix <NAME> --KeyDirectory <PATH>
```