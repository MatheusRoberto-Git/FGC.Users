# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos de projeto
COPY ["FGC.Users.Presentation/FGC.Users.Presentation.csproj", "FGC.Users.Presentation/"]
COPY ["FGC.Users.Application/FGC.Users.Application.csproj", "FGC.Users.Application/"]
COPY ["FGC.Users.Infrastructure/FGC.Users.Infrastructure.csproj", "FGC.Users.Infrastructure/"]
COPY ["FGC.Users.Domain/FGC.Users.Domain.csproj", "FGC.Users.Domain/"]

# Restore
RUN dotnet restore "FGC.Users.Presentation/FGC.Users.Presentation.csproj"

# Copia todo o código
COPY . .

# Build
WORKDIR "/src/FGC.Users.Presentation"
RUN dotnet build "FGC.Users.Presentation.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "FGC.Users.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FGC.Users.Presentation.dll"]