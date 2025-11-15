FROM  mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 9002

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build 

WORKDIR /src/
COPY . .

WORKDIR /src/Services/DespesaService
RUN dotnet publish "./DespesaService.csproj" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DespesaService.dll"]