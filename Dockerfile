# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia y restaura dependencias
COPY *.csproj ./
RUN dotnet restore

# Copia el resto del código
COPY . ./

# Compila en modo Release
RUN dotnet publish -c Release -o out

# Etapa de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out ./

# Cambiá Spotquickly.dll si el nombre es distinto
ENTRYPOINT ["dotnet", "Spotquickly.dll"]
