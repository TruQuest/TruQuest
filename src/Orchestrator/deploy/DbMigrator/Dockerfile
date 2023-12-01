FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src

COPY ./lib/Thataway/Thataway.csproj ./lib/Thataway/
COPY ./src/Domain/Domain.csproj ./src/Domain/
COPY ./src/Application/Application.csproj ./src/Application/
COPY ./src/Infrastructure/Infrastructure.csproj ./src/Infrastructure/
COPY ./src/API/API.csproj ./src/API/
COPY ./deploy/DbMigrator/DbMigrator.csproj ./deploy/DbMigrator/

RUN dotnet restore ./lib/Thataway/Thataway.csproj
RUN dotnet restore ./src/Domain/Domain.csproj
RUN dotnet restore ./src/Application/Application.csproj
RUN dotnet restore ./src/Infrastructure/Infrastructure.csproj
RUN dotnet restore ./src/API/API.csproj
RUN dotnet restore ./deploy/DbMigrator/DbMigrator.csproj

COPY ./lib/Thataway ./lib/Thataway
COPY ./src/Domain ./src/Domain
COPY ./src/Application ./src/Application
COPY ./src/Infrastructure ./src/Infrastructure
COPY ./src/API ./src/API
COPY ./deploy/DbMigrator ./deploy/DbMigrator

RUN dotnet publish ./deploy/DbMigrator/DbMigrator.csproj --no-restore -c Release -o /build/release

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /build/release .
RUN mkdir /app/artifacts
ENTRYPOINT ["dotnet", "DbMigrator.dll"]
