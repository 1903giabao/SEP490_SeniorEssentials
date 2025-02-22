# Base stage (Used in production & debugging)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Ensure we have root privileges
USER root

# Set working directory
WORKDIR /app

# Expose API on port 8081
EXPOSE 8081

# Set ASP.NET environment
ENV ASPNETCORE_ENVIRONMENT=Development

# Install required dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    tesseract-ocr \
    libtesseract-dev \
    libleptonica-dev \
    && rm -rf /var/lib/apt/lists/*  # Clean up cache to reduce image size

# Build stage (Compiles the service project)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

# Set working directory for source
WORKDIR /src

# Copy project files
COPY ["SE.API/SE.API.csproj", "SE.API/"]
COPY ["SE.Common/SE.Common.csproj", "SE.Common/"]
COPY ["SE.Data/SE.Data.csproj", "SE.Data/"]
COPY ["SE.Service/SE.Service.csproj", "SE.Service/"]

# Restore dependencies
RUN dotnet restore "./SE.API/SE.API.csproj"

# Copy the rest of the source files
COPY . .

# Set working directory to the API project
WORKDIR "/src/SE.API"

# Build the project
RUN dotnet build "./SE.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage (Prepares final app output)
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

# Publish the application
RUN dotnet publish "./SE.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage (Production-ready container)
FROM base AS final
WORKDIR /app

# Copy published app from previous stage
COPY --from=publish /app/publish .

# Verify that Tesseract is installed (Optional for debugging)
RUN tesseract --version

# Set entry point
ENTRYPOINT ["dotnet", "SE.API.dll"]
