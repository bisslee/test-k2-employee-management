# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Biss.EmployeeManagement.Api/Biss.EmployeeManagement.Api.csproj", "Biss.EmployeeManagement.Api/"]
COPY ["Biss.EmployeeManagement.Application/Biss.EmployeeManagement.Application.csproj", "Biss.EmployeeManagement.Application/"]
COPY ["Biss.EmployeeManagement.Domain/Biss.EmployeeManagement.Domain.csproj", "Biss.EmployeeManagement.Domain/"]
COPY ["Biss.EmployeeManagement.Infrastructure/Biss.EmployeeManagement.Infrastructure.csproj", "Biss.EmployeeManagement.Infrastructure/"]
COPY ["Biss.EmployeeManagement.CrossCutting/Biss.EmployeeManagement.CrossCutting.csproj", "Biss.EmployeeManagement.CrossCutting/"]

# Restore dependencies
RUN dotnet restore "Biss.EmployeeManagement.Api/Biss.EmployeeManagement.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Biss.EmployeeManagement.Api"
RUN dotnet build "Biss.EmployeeManagement.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Biss.EmployeeManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy published app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "Biss.EmployeeManagement.Api.dll"]
