# HelseID client key updater tool

```mermaid

graph TD;
    A[User] -->|Requests upade secret| B[Client Secret Tool];
    B -->|Authenticates with old key and clientId| C[HelseID];
    C -->|Authentication response| B;
    B -->|Registers new secret| D[HelseId selvbetjening];
    C -->|Confirmation & secret details| B;
    B -->|Returns secret| A;


```


## Commands

### Generate new keys

```
 generatekey --FileName <NAME> --KeyPath <PATH>
```

### Update client with new keys



```
 updateclientkey --ClientId <CLIENT_ID_> --NewKey <NEW_KEY_> --env dev --OldKey <OLD_KEY_>
```


```
 updateclientkey --ClientId <CLIENT_ID_> --NewKeyPath <PATH> --env dev --OldKeyPath <PATH>
```

## Publish new version
```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```
