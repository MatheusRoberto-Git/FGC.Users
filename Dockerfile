# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copia apenas os arquivos de projeto primeiro (otimiza cache)
COPY ["FGC.Users.Presentation/FGC.Users.Presentation.csproj", "FGC.Users.Presentation/"]
COPY ["FGC.Users.Application/FGC.Users.Application.csproj", "FGC.Users.Application/"]
COPY ["FGC.Users.Infrastructure/FGC.Users.Infrastructure.csproj", "FGC.Users.Infrastructure/"]
COPY ["FGC.Users.Domain/FGC.Users.Domain.csproj", "FGC.Users.Domain/"]

# Restore
RUN dotnet restore "FGC.Users.Presentation/FGC.Users.Presentation.csproj"

# Copia o restante do código
COPY . .

# Build
WORKDIR "/src/FGC.Users.Presentation"
RUN dotnet build "FGC.Users.Presentation.csproj" -c Release -o /app/build --no-restore

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "FGC.Users.Presentation.csproj" -c Release -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Stage 3: Runtime (Alpine)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

# Labels
LABEL maintainer="FIAP Cloud Games" \
      version="2.0.0" \
      description="FGC Users API - Microsserviço de Usuários"

# Instala curl para health check e icu-libs para globalização
RUN apk add --no-cache curl icu-libs

# Cria usuário non-root
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -D appuser

WORKDIR /app

# Copia os arquivos publicados
COPY --from=publish /app/publish .

# Altera ownership
RUN chown -R appuser:appgroup /app

# Muda para usuário non-root
USER appuser

EXPOSE 8080

# Variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "FGC.Users.Presentation.dll"]