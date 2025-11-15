FROM  mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
WORKDIR /app
EXPOSE 9000

# Instalar ping, curl e nslookup (dnsutils)
RUN apt-get update && apt-get install -y \
    iputils-ping \
    curl \
    dnsutils

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build 

WORKDIR /src
COPY . .

RUN dotnet publish "./api-gateway.csproj" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "api-gateway.dll"]