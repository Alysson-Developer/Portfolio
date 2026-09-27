FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Portfolio/Portfolio.csproj", "Portfolio/"]
RUN dotnet restore "Portfolio/Portfolio.csproj"

COPY Portfolio/ Portfolio/
WORKDIR /src/Portfolio
RUN dotnet publish "Portfolio.csproj" --configuration Release --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "exec dotnet Portfolio.dll --urls http://0.0.0.0:${PORT:-8080}"]
