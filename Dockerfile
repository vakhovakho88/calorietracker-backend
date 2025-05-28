# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore dependencies
COPY CalorieTracker.API/*.csproj ./CalorieTracker.API/
WORKDIR /app/CalorieTracker.API
RUN dotnet restore

# Copy the rest of the application code
WORKDIR /app
COPY CalorieTracker.API/ ./CalorieTracker.API/

# Build the application
WORKDIR /app/CalorieTracker.API
RUN dotnet publish -c Release -o /app/out

# Use the official .NET 8.0 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install SQLite (needed for Entity Framework SQLite)
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Copy the published application
COPY --from=build-env /app/out .

# Create directory for database
RUN mkdir -p /app/data

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:10000

# Update the connection string to use a persistent volume path
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/calorietracker.db"

# Expose the port that Render expects
EXPOSE 10000

# Create a non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

# Run the application
ENTRYPOINT ["dotnet", "CalorieTracker.API.dll"]
