FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/MarcasAutos.Api/MarcasAutos.Api.csproj", "src/MarcasAutos.Api/"]
RUN dotnet restore "src/MarcasAutos.Api/MarcasAutos.Api.csproj"

COPY ["src/MarcasAutos.Api/", "src/MarcasAutos.Api/"]
RUN dotnet publish "src/MarcasAutos.Api/MarcasAutos.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MarcasAutos.Api.dll"]
