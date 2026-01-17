# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["TrackIT.API/TrackIT.API.csproj", "TrackIT.API/"]
COPY ["TrackIT.Core/TrackIT.Core.csproj", "TrackIT.Core/"]
COPY ["TrackIT.Infrastructure/TrackIT.Infrastructure.csproj", "TrackIT.Infrastructure/"]
RUN dotnet restore "TrackIT.API/TrackIT.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/TrackIT.API"
RUN dotnet publish -c Release -o /app/publish

# 2. Serve Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
# This is crucial for Render to know which port to listen on
ENV ASPNETCORE_URLS=http://+:8080 
EXPOSE 8080
ENTRYPOINT ["dotnet", "TrackIT.API.dll"]