FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src

COPY ./deploy/ContractMigrator/ContractMigrator.csproj ./deploy/ContractMigrator/
RUN dotnet restore ./deploy/ContractMigrator/ContractMigrator.csproj

COPY ./deploy/ContractMigrator ./deploy/ContractMigrator

RUN dotnet publish ./deploy/ContractMigrator/ContractMigrator.csproj --no-restore -c Release -o /build/release

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /build/release .
RUN mkdir /app/artifacts
ENTRYPOINT ["dotnet", "ContractMigrator.dll"]
