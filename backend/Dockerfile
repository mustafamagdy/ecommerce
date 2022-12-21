FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
COPY ["Directory.Build.props", "/"]
COPY ["Directory.Build.targets", "/"]
COPY ["dotnet.ruleset", "/"]
COPY ["stylecop.json", "/"]
WORKDIR /src
COPY ["src/Host/Host.csproj", "Host/"]
COPY ["src/Core/Application/Application.csproj", "Core/Application/"]
COPY ["src/Core/Domain/Domain.csproj", "Core/Domain/"]
COPY ["src/Core/Shared/Shared.csproj", "Core/Shared/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["src/Migrators/Migrators.MySQL/Migrators.MySQL.csproj", "Migrators/Migrators.MySQL/"]
COPY ["src/Migrators/Migrators.PostgreSQL/Migrators.PostgreSQL.csproj", "Migrators/Migrators.PostgreSQL/"]
COPY ["src/Migrators/Migrators.MSSQL/Migrators.MSSQL.csproj", "Migrators/Migrators.MSSQL/"]
RUN dotnet restore "Host/Host.csproj"
COPY . .
WORKDIR "/src/Host"
#RUN dotnet build "Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Host.csproj" -c Release -o /app/publish

FROM base AS final
RUN apt-get update
RUN apt-get install freetype-devel glib2-devel cairo-devel
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FSH.WebApi.Host.dll"]
