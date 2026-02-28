# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for restore
COPY Libertad.sln ./
COPY src/Libertad.Api/Libertad.Api.csproj src/Libertad.Api/
COPY src/Libertad.Application/Libertad.Application.csproj src/Libertad.Application/
COPY src/Libertad.Contracts/Libertad.Contracts.csproj src/Libertad.Contracts/
COPY src/Libertad.Domain/Libertad.Domain.csproj src/Libertad.Domain/
COPY src/Libertad.Infrastructure/Libertad.Infrastructure.csproj src/Libertad.Infrastructure/

RUN dotnet restore src/Libertad.Api/Libertad.Api.csproj

# Copy full source and publish
COPY src/ ./src/
RUN dotnet publish src/Libertad.Api/Libertad.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Libertad.Api.dll"]
