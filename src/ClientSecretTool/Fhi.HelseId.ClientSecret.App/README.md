# HelseID client key updater tool

## Commands

### Generate new keys

```
 generatekey --FileName <NAME> --KeyPath <PATH>
```

### Update client with new keys

```
 generatekey --FileName <NAME> --KeyPath <PATH>
```

## Publish new version
```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```
