FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src

COPY ["FileArchiveService.csproj", "FileArchiveService.csproj"]
RUN dotnet restore
COPY . .
RUN dotnet publish --no-restore -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:7.0
RUN mkdir /images
RUN mkdir /user_files
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT [ "dotnet", "FileArchiveService.dll" ]
