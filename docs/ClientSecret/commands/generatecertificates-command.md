# Generate keys in certificate format Commands

## Parameters

| Parameter name        | Short name| Description																					| Required | Sample						|
|-----------------------|----------| -----------------------------------------------------------------------------------------------|----------|-------------------------------|
| CertificateCommonName |cn | Common name of the certificate, key file name prefix. <br><br>Files generated: <br>`<CommonName>_private.pfx`<br> `<CommonName>_public.pem`<br>  `<CommonName>_thumbprint.txt`|<b>Yes</b>|`"newCertificate"`|
| CertificatePassword   |pwd |Password for the private certificate.|<b>Yes</b>|`"CertPassword123!"`|
| CertificateDirectory  |dir |Path to where private and public key will be stored.|<b>No</b>|`"C:\\temp"`|


## Commands
```
 generatecertificate --CertificateCommonName <NAME> --CertificatePassword <PASSWORD> --CertificateDirectory <PATH>
```

## Notes
- Public certificate in PEM format can be added to client in HelseId selvbetjening 
- Private certificate can be installed on server or for users as needed (requires certificate password)
- Programmatic retrieval of certificate requires input of certificate thumbprint

## Code sample

See code lab [Generate Certificates](../code-lab/generatecertificates.ipynb) 