FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src

COPY ./lib/Thataway/Thataway.csproj ./lib/Thataway/
COPY ./src/Domain/Domain.csproj ./src/Domain/
COPY ./src/Application/Application.csproj ./src/Application/
COPY ./src/Infrastructure/Infrastructure.csproj ./src/Infrastructure/
COPY ./src/API/API.csproj ./src/API/

RUN dotnet restore ./lib/Thataway/Thataway.csproj
RUN dotnet restore ./src/Domain/Domain.csproj
RUN dotnet restore ./src/Application/Application.csproj
RUN dotnet restore ./src/Infrastructure/Infrastructure.csproj
RUN dotnet restore ./src/API/API.csproj

COPY ./lib/Thataway ./lib/Thataway
COPY ./src/Domain ./src/Domain
COPY ./src/Application ./src/Application
COPY ./src/Infrastructure ./src/Infrastructure
COPY ./src/API ./src/API

RUN dotnet publish ./src/API/API.csproj --no-restore -c Release -o /build/release

# @@??: Do not copy wwwroot?

FROM mcr.microsoft.com/dotnet/aspnet:7.0
RUN apt-get update -y && apt-get install -y curl
WORKDIR /app
EXPOSE 5223
COPY --from=build /build/release .
ENTRYPOINT ["dotnet", "API.dll"]
