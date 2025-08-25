# Update client with new keys Commands

## Flow

1. **User Request:** A user initiates a request to upgrade/ rotate the client secret for an application through the tools console interface.
2. **Authentication:** The Client Secret Tool authenticates with Helse ID using the old secret and client ID to verify its identity to get an access token.
3. **Secret Update Process:** The tool updates the client configuration through the Helse ID Selvbetjening API passing the access token. The updated secret is then stored in the Client Configuration inside Helse ID.


## Parameters

|Parameter name | Descrition																					| Required | Sample						|
|---------------|-----------------------------------------------------------------------------------------------|----------|-------------------------------|
|ClientId		|The target client for the secret update. <br> Found in HelseId Selvbetjening Klient konfigurasjon. | <b>Yes</b> | `37a08838-db82-4de0-bfe1-bed876e7086e` |
|NewPublicJwkPath|Path to the new public key.                                                                   | <b>No*</b> | `C:\keys\37a08838-db82-4de0-bfe1-bed876e7086e_public.json`|
|NewPublicJwk	|The new public key string.                                                                             | <b>No*</b> | `{\"alg\":\"PS512\",\"d\":\"xxx .....}`|
|ExistingPrivateJwkPath	|Path to the existing private key.                                                      | <b>No*</b> |`C:\keys\37a08838-db82-4de0-bfe1-bed876e7086e_private.json`|
|ExistingPrivateJwk	|The old private key string.                                                                        | <b>No*</b> |`{\"alg\":\"PS512\",\"d\":\"xxx .....}`|
|yes|Automatic confirmation of update without user input.                                                                        | No |--Yes|

<i>*Either path or string parameter must be provided.</i>

## Commands

### updateclientkey using key string
```
 updateclientkey --ClientId <CLIENT_ID> --NewPublicJwk <NEW_KEY>  --ExistingPrivateJwk <OLD_KEY>
```
### updateclientkey using key path
```
 updateclientkey --ClientId <CLIENT_ID> --NewPublicJwkPath <PATH> --ExistingPrivateJwkPath <PATH>
```
### updateclientkey with automatic confirmation
```
 updateclientkey --ClientId <CLIENT_ID> --NewPublicJwk <NEW_KEY>  --ExistingPrivateJwk <OLD_KEY> --Yes
```

## Code sample

See code lab [Update client secret](../code-lab/updateclientsecret.ipynb) 