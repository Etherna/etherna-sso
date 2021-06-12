FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "EthernaSSO.sln"
RUN dotnet build "EthernaSSO.sln" -c Release -o /app/build
RUN dotnet test "EthernaSSO.sln" -c Release

FROM build AS publish
RUN dotnet publish "EthernaSSO.sln" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EthernaSSO.dll"]