FROM chekhenkho/ubuntu_xvfb:0.1.0 AS base

WORKDIR /singlefile

COPY ublock/ ublock/
COPY cookies/ cookies/
COPY bootstrap.sh bootstrap.sh
COPY singlefile.sh singlefile.sh
RUN chmod 755 bootstrap.sh
RUN chmod 755 singlefile.sh

RUN mkdir input
RUN mkdir output
RUN mkdir /images
RUN mkdir /user_files

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src/FileArchiveService

COPY ["FileArchiveService.csproj", "FileArchiveService.csproj"]

RUN dotnet restore

COPY Attributes/ Attributes/
COPY Common/ Common/
COPY Messages/ Messages/
COPY Properties/ Properties/
COPY Services/ Services/
COPY Utils/ Utils/
COPY *.json ./
COPY *.cs ./

RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final
ENV DOTNET_ENVIRONMENT Development
ENV ASPNETCORE_ENVIRONMENT Development
COPY --from=build /app /app
ENTRYPOINT [ "/singlefile/bootstrap.sh" ]
