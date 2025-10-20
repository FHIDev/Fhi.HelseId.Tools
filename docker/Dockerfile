#FROM ubuntu:latest

# Install prerequisites
#RUN apt-get update && apt-get install -y wget apt-transport-https

# Install Micrososft stuff
#RUN wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    #&& dpkg -i packages-microsoft-prod.deb \
    #&& rm packages-microsoft-prod.deb

# Install .NET SDK
#RUN apt-get update && apt-get install -y dotnet-sdk-8.0

# Set working directory
#WORKDIR /script

# Copy your script
#COPY Key-gen-and-update.sh .

# Make it executable
#RUN chmod +x Key-gen-and-update.sh

# Run the script
#CMD ["./Key-gen-and-update.sh"]

# -----------------------------------

FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /script

COPY Key-gen-and-update.sh .

RUN chmod +x Key-gen-and-update.sh

CMD ["./Key-gen-and-update.sh"]