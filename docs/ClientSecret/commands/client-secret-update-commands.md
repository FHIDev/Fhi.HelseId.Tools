# Update client with new keys Commands

## Flow

1. **User Request:** A user initiates a request to upgrade/rotate the client secret for an application through the tools console interface.
2. **Authentication:** The Client Secret Tool authenticates with Helse ID using the old secret and client ID to verify its identity to get an access token.
3. **Secret Update Process:** The tool updates the client configuration throuhg the Helse ID Selvbetjening API passing the access token. The updated secret is then stored in the Client Configuration inside Helse ID.


## Parameters

|Parameter name | Descrition																					| Sample						|
|---------------|-----------------------------------------------------------------------------------------------|-------------------------------|
|ClientId		| The Client to updates unique Identifier found in Klient konfigurasjon in HelseId Selvbetjening| `37a08838-db82-4de0-bfe1-bed876e7086e` |
|NewPublicJwkPath|Path to the new public key.                                                                   | `C:\keys\37a08838-db82-4de0-bfe1-bed876e7086e_public.json`|
|NewPublicJwk	|public key string                                                                              | `{\"alg\":\"PS512\",\"d\":\"xxx .....}`|
|ExistingPrivateJwkPath	|Path to the new private key|`C:\keys\37a08838-db82-4de0-bfe1-bed876e7086e_private.json`|
|ExistingPrivateJwk	|private key string|`{\"alg\":\"PS512\",\"d\":\"xxx .....}`|


## Commands

```
 updateclientkey --ClientId <CLIENT_ID> --NewPublicJwk <NEW_KEY>  --ExistingPrivateJwk <OLD_KEY>
```


```
 updateclientkey --ClientId <CLIENT_ID> --NewPublicJwkPath <PATH> --ExistingPrivateJwkPath <PATH>
```

## Code sample

See code lab [Update client secret](../code-lab/updateclientsecret.ipynb) 