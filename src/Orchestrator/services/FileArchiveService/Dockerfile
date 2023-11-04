FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src
COPY ["FileArchiveService.csproj", "FileArchiveService.csproj"]
RUN dotnet restore
COPY . .
RUN dotnet publish --no-restore -c Release -o /app

FROM playwright:0.1.0 AS base
WORKDIR /app
RUN mkdir /images
RUN mkdir /user_files
RUN mkdir /screenshots
COPY --from=build /app .
ENTRYPOINT ["dotnet", "FileArchiveService.dll"]
