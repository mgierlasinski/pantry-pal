# Stage 1: Build
# This stage uses the .NET SDK to build and publish the application.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files to leverage Docker layer caching
COPY ["PantryPal.sln", "src/PantryPal.Api/PantryPal.Api.csproj", "src/PantryPal.Data/PantryPal.Data.csproj", "./"]
COPY ["src/PantryPal.Api/", "src/PantryPal.Api/"]
COPY ["src/PantryPal.Data/", "src/PantryPal.Data/"]

# Restore dependencies for the API project only
RUN dotnet restore "src/PantryPal.Api/PantryPal.Api.csproj"

# Copy the rest of the application source code
COPY . .
WORKDIR "/src/src/PantryPal.Api"

# Publish the API project specifically
RUN dotnet publish "PantryPal.Api.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Final
# This stage uses the lightweight ASP.NET runtime image for the final container.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Create a non-root user for security
RUN adduser -u 1000 --disabled-password --gecos '' appuser
USER appuser

# Expose the port the app will run on
EXPOSE 8080

# Configure environment variables for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080

# Healthcheck to ensure the container is running correctly.
# Note: You need to configure a health check endpoint at /healthz in your application.
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/healthz || exit 1

# Entrypoint to run the application
ENTRYPOINT ["dotnet", "PantryPal.Api.dll"]
