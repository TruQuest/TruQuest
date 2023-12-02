FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src

COPY Dummy.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish --no-restore -c Release -o /build/release

FROM mcr.microsoft.com/dotnet/aspnet:7.0
RUN apt-get update -y && apt-get install -y curl
EXPOSE 5223
WORKDIR /app
COPY --from=build /build/release .
ENTRYPOINT ["dotnet", "Dummy.dll"]
