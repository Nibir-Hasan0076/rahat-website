FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY StyleNest.csproj ./
RUN dotnet restore StyleNest.csproj
COPY . .
RUN dotnet publish StyleNest.csproj -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /out .

# The runtime image runs as the non-root "app" user; give it ownership of the
# app files and the SQLite data directory.
USER root
RUN mkdir -p /data && chown -R app:app /app /data
USER app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:10000 \
    ConnectionStrings__DefaultConnection="Data Source=/data/stylenest.db"

EXPOSE 10000
ENTRYPOINT ["dotnet", "StyleNest.dll"]
