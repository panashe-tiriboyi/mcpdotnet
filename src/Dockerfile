# Dockerfile for DataBalk.Mcp.Server.Copilot (.NET 9.0)
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and restore as distinct layers
COPY ../DataBalk.Mcp.sln ./
COPY ./DataBalk.Mcp.Server.Copilot/*.csproj ./DataBalk.Mcp.Server.Copilot/
COPY ./DataBalk.Mcp.Common/*.csproj ./DataBalk.Mcp.Common/
COPY ./DataBalk.Mcp.Client/*.csproj ./DataBalk.Mcp.Client/
RUN dotnet restore ./DataBalk.Mcp.Server.Copilot/DataBalk.Mcp.Server.Copilot.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish ./DataBalk.Mcp.Server.Copilot/DataBalk.Mcp.Server.Copilot.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port (default ASP.NET Core port)
EXPOSE 80

# Set environment variables for Render
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "DataBalk.Mcp.Server.Copilot.dll"]
