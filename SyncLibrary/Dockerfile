FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY Synker.sln ./
COPY SyncLibrary/hfa.Synker.batch.csproj SyncLibrary/
COPY hfa.TvhLibrary/hfa.TvhLibrary.csproj hfa.TvhLibrary/
COPY PlaylistBaseLibrary/hfa.PlaylistBaseLibrary.csproj PlaylistBaseLibrary/
COPY hfa.Synker.Service/hfa.Synker.Service.csproj hfa.Synker.Service/
COPY Brokers/hfa.Notification.DTo/hfa.Brokers.Messages.csproj Brokers/hfa.Notification.DTo/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/SyncLibrary
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "hfa.Synker.batch.dll"]
