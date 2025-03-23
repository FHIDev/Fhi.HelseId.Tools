# HelseID client key updater tool

## Description
Client Secret Tool is a console application that facilitates the secure rotation of client secrets for applications integrated with Helse ID.
The tool also provide generation of new private and public Json Web Keys (jwk)


## Generate new keys Commands

```
 generatekey --FileName <NAME> --KeyPath <PATH>
```

## Update client with new keys Commands

```
 updateclientkey --ClientId <CLIENT_ID> --NewClientJwk <NEW_KEY> --env dev --OldClientJwk <OLD_KEY>
```


```
 updateclientkey --ClientId <CLIENT_ID> --NewClientJwkPath <PATH> --env dev --OldClientJwkPath <PATH>
```