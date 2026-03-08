# ── Etapa 1: Build ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia toda la solución
COPY . .

RUN dotnet publish ProductComparisonApi.API/ProductComparisonApi.API.csproj \
    -c Release \
    -o /app/out

# ── Etapa 2: Runtime ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/out .

COPY ProductComparisonApi.Infrastructure/Data/products.json /app/products.json.default

COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["/app/entrypoint.sh"]