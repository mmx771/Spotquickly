# Imagen base para compilar el proyecto
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar archivos del proyecto
COPY *.csproj ./
RUN dotnet restore

# Copiar el resto del código y compilar
COPY . ./
RUN dotnet publish -c Release -o out

# Imagen final (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out .

# Expone el puerto para Render (aunque Render lo detecta solo)
EXPOSE 80

# Ejecutar la app
ENTRYPOINT ["dotnet", "Spotquickly.dll"]

