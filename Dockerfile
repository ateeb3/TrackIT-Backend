# 1. Build Stage
# ✅ FIX: Updated to .NET 10.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["TrackIT.API/TrackIT.API.csproj", "TrackIT.API/"]
COPY ["TrackIT.Core/TrackIT.Core.csproj", "TrackIT.Core/"]
COPY ["TrackIT.Data/TrackIT.Data.csproj", "TrackIT.Data/"] 

# Restore dependencies
RUN dotnet restore "TrackIT.API/TrackIT.API.csproj"

# Copy the rest of the source code
COPY . .

# Build and Publish
WORKDIR "/src/TrackIT.API"
RUN dotnet publish -c Release -o /app/publish

# 2. Runtime Stage
# ✅ FIX: Updated to .NET 10.0 Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Environment setup for Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TrackIT.API.dll"]