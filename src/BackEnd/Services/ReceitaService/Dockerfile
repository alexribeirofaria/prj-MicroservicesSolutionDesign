FROM  mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 9003

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build 

WORKDIR /src/
COPY ./src .

WORKDIR /src/BackEnd/Services/ReceitaService
RUN dotnet publish "./ReceitaService.csproj" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ReceitaService.dll"]