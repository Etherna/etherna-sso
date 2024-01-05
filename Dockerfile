FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
RUN curl -SLO https://deb.nodesource.com/nsolid_setup_deb.sh
RUN chmod 500 nsolid_setup_deb.sh
RUN ./nsolid_setup_deb.sh 20
RUN apt-get install nodejs -y
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