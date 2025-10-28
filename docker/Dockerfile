FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR /script

ENV PATH="$PATH:/root/.dotnet/tools"

ENV CLIENT_ID="..."

RUN dotnet tool install --global Fhi.HelseIdSelvbetjening.CLI --version 1.0.0

RUN apt-get update && \
    apt-get install -y curl apt-transport-https lsb-release gnupg && \
    curl -sL https://aka.ms/InstallAzureCLIDeb | bash

COPY Key-gen-and-update.sh .
RUN chmod +x Key-gen-and-update.sh

CMD ["bash", "./Key-gen-and-update.sh"]