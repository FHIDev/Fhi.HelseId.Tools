# HelseID CLI Tool

Command line tool for HelseID client configuration and key management.

## Features

- Generate RSA key pairs for use with HelseID
- Update client keys in HelseID
- Generates self-signed certificates as an alternative to RSA keys for HelseID

## Installation

### As a .NET Tool

This application is packaged as a .NET tool and can be installed either globally or locally.

#### Global Installation

```bash
# Install globally from NuGet
dotnet tool install --global Fhi.HelseIdSelvbetjening.CLI

# Use from anywhere
helseid-cli --help
```

#### Local Installation

```bash
# Create a tool manifest in your project if you don't have one
dotnet new tool-manifest

# Install as a local tool from NuGet
dotnet tool install Fhi.HelseIdSelvbetjening.CLI

# Use in the project
dotnet tool run helseid-cli --help
```

### Development Installation

If you're developing this tool or want to install from a local build:

```bash
# Install globally from a local source
dotnet tool install --global --add-source ./src/Fhi.HelseIdSelvbetjening.CLI/nupkg Fhi.HelseIdSelvbetjening.CLI

# Or install locally from a local source
dotnet tool install --add-source ./src/Fhi.HelseIdSelvbetjening.CLI/nupkg Fhi.HelseIdSelvbetjening.CLI
```

### Manual Installation

If you prefer to run the tool without installing it as a .NET tool:

```bash
# Clone the repository
git clone https://github.com/folkehelseinstituttet/Fhi.HelseId.Tools.git

# Navigate to the CLI project
cd Fhi.HelseId.Tools/src/Fhi.HelseIdSelvbetjening.CLI

# Run the application
dotnet run -- --help
```

## Usage

### Generate Key Command

Generate a new RSA key pair:

```bash
helseid-cli generatekey --keyFileNamePrefix MyClient --keyDirectory C:\Keys
```

Or with short options:

```bash
helseid-cli generatekey -n MyClient -d C:\Keys
```

### Update Client Key Command

Update a client key in HelseID:

```bash
helseid-cli updateclientkey --clientId "your-client-id" --newPublicJwkPath "path/to/public.json" --existingPrivateJwkPath "path/to/private.json"
```

Or by providing the key values directly:

```bash
helseid-cli updateclientkey --clientId "your-client-id" --newPublicJwk "{...jwk json...}" --existingPrivateJwk "{...jwk json...}"
```

## Packaging

To package the application as a NuGet package:

```bash
# Navigate to the CLI project
cd src/Fhi.HelseIdSelvbetjening.CLI

# Create the NuGet package
dotnet pack

# The package will be created in the ./nupkg directory
```

## License

MIT