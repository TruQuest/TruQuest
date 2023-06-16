FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src

COPY ./lib/ContractStorageExplorer/src/ContractStorageExplorer.csproj ./lib/ContractStorageExplorer/src/
COPY ./src/Domain/Domain.csproj ./src/Domain/
COPY ./src/Application/Application.csproj ./src/Application/
COPY ./src/Infrastructure/Infrastructure.csproj ./src/Infrastructure/
COPY ./src/API/API.csproj ./src/API/
COPY ./deploy/DbMigrator/DbMigrator.csproj ./deploy/DbMigrator/

RUN dotnet restore ./lib/ContractStorageExplorer/src/ContractStorageExplorer.csproj
RUN dotnet restore ./src/Domain/Domain.csproj
RUN dotnet restore ./src/Application/Application.csproj
RUN dotnet restore ./src/Infrastructure/Infrastructure.csproj
RUN dotnet restore ./src/API/API.csproj
RUN dotnet restore ./deploy/DbMigrator/DbMigrator.csproj

COPY ./lib/ContractStorageExplorer/src ./lib/ContractStorageExplorer/src
COPY ./src/Domain ./src/Domain
COPY ./src/Application ./src/Application
COPY ./src/Infrastructure ./src/Infrastructure
COPY ./src/API ./src/API
COPY ./deploy/DbMigrator ./deploy/DbMigrator

RUN dotnet publish ./deploy/DbMigrator/DbMigrator.csproj --no-restore -c Release -o /build/release

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /build/release .
ENTRYPOINT ["dotnet", "DbMigrator.dll"]
