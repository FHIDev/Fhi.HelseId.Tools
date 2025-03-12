# HelseID client key updater tool

## Commands

### Generate new keys

```
 dotnet run --ClientId=<clientId> --KeyPath=C:\Keys
```

### Update client with new keys

```
 dotnet run --ClientId=<clientId> --KeyPath=C:\Keys
```

## Publish new version
```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```
