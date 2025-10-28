# PROOF OF CONCEPT
# This image is intended to be ran as a scheduled job

FROM mcr.microsoft.com/dotnet/sdk:9.0

# Sets the working directory
WORKDIR /script

# Ensures that dotnet is usable on the path
ENV PATH="$PATH:/root/.dotnet/tools"

# Example of how to use a environment variable directly in dockerfile
# Use with Skybert is a little different, but works similarly
ENV CLIENT_ID="123"
# ENV PRIVATE_KEY="$Private_key" or similar, Skybert has built-in mechanisms to get keys from keyvault
# ENV PUBLIC_KEY="$Public_key" or similar, Skybert has built-in mechanisms to get keys from keyvault

# Installs the tool (maybe the tool version also should be a variable?)
RUN dotnet tool install --global Fhi.HelseIdSelvbetjening.CLI --version 1.0.0

# Installs the azure commands for use with azure keyvault
RUN apt-get update && \
    apt-get install -y curl apt-transport-https lsb-release gnupg && \
    curl -sL https://aka.ms/InstallAzureCLIDeb | bash

# Adds the script file and makes it executable
COPY Key-gen-and-update.sh .
RUN chmod +x Key-gen-and-update.sh

# Runs the bash script and container dies when the script finishes
CMD ["bash", "./Key-gen-and-update.sh"]
# Any commandline logs in the script is automatically picked up in the Skybert logs