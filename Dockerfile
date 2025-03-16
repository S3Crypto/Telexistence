FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/TelexistenceAPI/TelexistenceAPI.csproj", "src/TelexistenceAPI/"]
COPY ["src/TelexistenceAPI.Core/TelexistenceAPI.Core.csproj", "src/TelexistenceAPI.Core/"]
RUN dotnet restore "src/TelexistenceAPI/TelexistenceAPI.csproj"
COPY . .
WORKDIR "/src/src/TelexistenceAPI"
RUN dotnet build "TelexistenceAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TelexistenceAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelexistenceAPI.dll"]