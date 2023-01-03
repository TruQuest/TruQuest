FROM ubuntu:focal AS base

RUN apt update; apt clean

RUN apt install -y xvfb fluxbox wget wmctrl gnupg2 curl

RUN wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && echo "deb http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list
RUN apt update && apt -y install google-chrome-stable

RUN curl -fsSL https://deb.nodesource.com/setup_16.x | bash -
RUN apt install -y nodejs
RUN npm install -g --production single-file-cli

RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb
RUN apt update && apt install -y aspnetcore-runtime-7.0

WORKDIR /singlefile

COPY ublock/ ublock/
COPY bootstrap.sh bootstrap.sh
COPY singlefile.sh singlefile.sh
RUN chmod 755 bootstrap.sh
RUN chmod 755 singlefile.sh

RUN mkdir input
RUN mkdir output
RUN mkdir /images

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src/FileArchiveService

COPY ["FileArchiveService.csproj", "FileArchiveService.csproj"]

RUN dotnet restore

COPY . .

RUN dotnet publish --no-restore -c Release -o /app

FROM build AS publish

FROM base AS final
ENV DOTNET_ENVIRONMENT Development
ENV ASPNETCORE_ENVIRONMENT Development
COPY --from=publish /app /app
ENTRYPOINT [ "/singlefile/bootstrap.sh" ]
