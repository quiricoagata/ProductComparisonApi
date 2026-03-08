#!/bin/bash

echo "Verificando archivo de datos..."

# Si el volumen está vacío o es el primer deploy,
# copia el products.json por defecto al volumen
if [ ! -f "/app/Data/products.json" ]; then
    echo "Archivo no encontrado. Inicializando desde backup..."
    mkdir -p /app/Data
    cp /app/products.json.default /app/Data/products.json
    echo "products.json inicializado correctamente."
else
    echo "Archivo encontrado. Usando datos existentes."
fi

# Arranca la aplicación .NET
exec dotnet ProductComparisonApi.API.dll