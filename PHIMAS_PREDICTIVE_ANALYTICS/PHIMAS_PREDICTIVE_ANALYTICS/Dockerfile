FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY PHIMAS_PREDICTIVE_ANALYTICS.csproj ./
RUN dotnet restore ./PHIMAS_PREDICTIVE_ANALYTICS.csproj

COPY . ./
RUN dotnet publish ./PHIMAS_PREDICTIVE_ANALYTICS.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_EnableDiagnostics=0

# 🔥 IMPORTANT FOR RENDER
ENV ASPNETCORE_URLS=http://+:$PORT

EXPOSE 8080

ENTRYPOINT ["dotnet", "PHIMAS_PREDICTIVE_ANALYTICS.dll"]