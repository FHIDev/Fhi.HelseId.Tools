# Fhi.HelseId.Tools
Developer and operations tools to work with HelseID clients and registrations for FHI


## Tool for update HelseIdcClient secrets (Jwk)

### Update client with new keys

In order for an application to use the tool the Client must have registered _nhn:selvbetjening/client_ scope. Below is flow of how dataflow works

```mermaid
flowchart LR

User("<img src='https://super.so/icon/dark/user.svg'; width='25' /> User")

subgraph ClientSecretTool[Client secret tool]
end

subgraph HelseID[Helse ID]
    subgraph OIDCProvider[OIDC Provider]
        subgraph ClientConfig[Client configuration<br>Scope: _nhn:selvbetjening/client_]
        end
    end
    
    subgraph HelseIDSelvbetjening[Helse ID Selvbetjening]
    end
end

User --- |Requests upgrade secret| ClientSecretTool;
ClientSecretTool --- |**1:** Authenticates with <br> old keyand clientId | OIDCProvider
ClientSecretTool --- |**2a:** Updates client with new secret | HelseIDSelvbetjening
HelseIDSelvbetjening --- |**2b:** Update Client configuration<br>with new Key| ClientConfig

```

1. **User Request:** A user initiates a request to upgrade/rotate the client secret for an application through the tools console interface.
2. **Authentication:** The Client Secret Tool authenticates with Helse ID using the old secret and client ID to verify its identity to get an access token.
3. **Secret Update Process:** The tool updates the client configuration throuhg the Helse ID Selvbetjening API passing the access token. The updated secret is then stored in the Client Configuration inside Helse ID.


