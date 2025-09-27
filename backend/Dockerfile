FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/StreetRacer.Api/StreetRacer.Api.csproj", "src/StreetRacer.Api/"]
COPY ["src/StreetRacer.Application/StreetRacer.Application.csproj", "src/StreetRacer.Application/"]
COPY ["src/StreetRacer.Domain/StreetRacer.Domain.csproj", "src/StreetRacer.Domain/"]
COPY ["src/StreetRacer.Infrastructure/StreetRacer.Infrastructure.csproj", "src/StreetRacer.Infrastructure/"]
COPY ["src/StreetRacer.Workers/StreetRacer.Workers.csproj", "src/StreetRacer.Workers/"]

# Restore dependencies
RUN dotnet restore "src/StreetRacer.Api/StreetRacer.Api.csproj"

# Copy source code
COPY . .

# Build application
WORKDIR "/src/src/StreetRacer.Api"
RUN dotnet build "StreetRacer.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StreetRacer.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StreetRacer.Api.dll"]