FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src

COPY Dummy.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish --no-restore -c Release -o /build/release

FROM mcr.microsoft.com/dotnet/aspnet:7.0
EXPOSE 80
WORKDIR /app
COPY --from=build /build/release .
ENTRYPOINT ["dotnet", "Dummy.dll"]