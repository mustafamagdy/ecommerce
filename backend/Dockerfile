FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /

# Copy csproj and restore as distinct layers
COPY ["Directory.Build.props", "/"]
COPY ["Directory.Build.targets", "/"]
COPY ["dotnet.ruleset", "/"]
COPY ["stylecop.json", "/"]
COPY ["src/Host/Host.csproj", "src/Host/"]
COPY ["src/Core/Application/Application.csproj", "src/Core/Application/"]
COPY ["src/Core/Domain/Domain.csproj", "src/Core/Domain/"]
COPY ["src/Core/Shared/Shared.csproj", "src/Core/Shared/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Migrators/Migrators.MySQL/Migrators.MySQL.csproj", "src/Migrators/Migrators.MySQL/"]

RUN dotnet restore "src/Host/Host.csproj" --disable-parallel
#RUN dotnet tool install --tool-path . dotnet-reportgenerator-globaltool --ignore-failed-sources

# Copy everything else and build
COPY . .
WORKDIR "/src/Host"
RUN dotnet publish "Host.csproj" -c Release -o /app/publish

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
RUN apt-get update
RUN apt-get install -y libfreetype6
RUN apt-get install -y libfontconfig1
RUN apt-get install -y vim
WORKDIR /app

COPY --from=build /app/publish .

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
#RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
#USER appuser
#ENV ASPNETCORE_URLS=https://+:5050;http://+:5060
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "FSH.WebApi.Host.dll"]