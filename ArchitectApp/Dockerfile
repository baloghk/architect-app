# STAGE 1: "BUILD"
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copying project files for dependecy caching
COPY ["src/ArchitectApp/ArchitectApp.csproj", "src/ArchitectApp/"]
COPY ["src/ArchitectApp/ClientApp/package.json", "src/ArchitectApp/ClientApp/"]
COPY ["src/ArchitectApp/ClientApp/package-lock.json", "src/ArchitectApp/ClientApp/"]

# 2. Restoring .NET dependecies (NuGet packages)
RUN dotnet restore "src/ArchitectApp/ArchitectApp.csproj"

# 3. Restoring Node.js dependecies (node_modules)
RUN npm install --prefix "src/ArchitectApp/ClientApp"

# 4. Copying the source code
COPY . .

# 5. Frontend Build (Vite)
# wwwroot/dist folder will be generated here
RUN npm run build --prefix "src/ArchitectApp/ClientApp"

# 6. Publishing .NET App
# compiles source code , collects wwwroot directory
RUN dotnet publish "src/ArchitectApp/ArchitectApp.csproj" -c Release -o /app/publish /p:UseAppHost=false


# STAGE 2: "FINAL"
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# 7. Copying the finished app from "build" stage
COPY --from=build /app/publish .

# 8. Execution command
ENTRYPOINT ["dotnet", "ArchitectApp.dll"]