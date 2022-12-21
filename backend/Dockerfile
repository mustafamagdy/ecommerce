FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Host/Host.csproj", "Host/"]
COPY ["src/Core/Application/Application.csproj", "Application/"]
COPY ["src/Core/Domain/Domain.csproj", "Domain/"]
COPY ["src/Core/Shared/Shared.csproj", "Shared/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["src/Migrators/Migrators.MySQL/Migrators.MySQL.csproj", "Migrators.MySQL/"]
COPY ["src/Migrators/Migrators.PostgreSQL/Migrators.PostgreSQL.csproj", "Migrators.PostgreSQL/"]
COPY ["src/Migrators/Migrators.MSSQL/Migrators.MSSQL.csproj", "Migrators.MSSQL/"]
RUN dotnet restore "Host/Host.csproj"
COPY . .
WORKDIR "/src/Host"
RUN dotnet build "Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Host.csproj" -c Release -o /app/publish

FROM base AS final
RUN apt-get update
RUN apt-get install -y libfreetype6
RUN apt-get install -y libfontconfig1
RUN apt-get install -y vim
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FSH.WebApi.Host.dll"]