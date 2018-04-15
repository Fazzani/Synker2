ARG arch=x64
FROM microsoft/aspnetcore:2.0 AS base
ARG arch
LABEL maintainer="synker-team@synker.ovh" \
      description="Batch Synker suite application" \
      system.dist="linux" system.arch="$arch" multi.name="Synker Batch"
#RUN apt-get -qq update && apt-get -y upgrade
# Create Group and user
#RUN groupadd -r synker && useradd --no-log-init -r -g synker synker
#USER synker:synker
# add user and group permissions
WORKDIR /app

ARG arch=x64
FROM microsoft/aspnetcore-build:2.0 AS build
ARG arch
WORKDIR /src
COPY Synker.sln ./
COPY SyncLibrary/hfa.Synker.batch.csproj SyncLibrary/
COPY hfa.TvhLibrary/hfa.TvhLibrary.csproj hfa.TvhLibrary/
COPY PlaylistBaseLibrary/hfa.PlaylistBaseLibrary.csproj PlaylistBaseLibrary/
COPY hfa.Synker.Service/hfa.Synker.Service.csproj hfa.Synker.Service/
COPY hfa.Notification.DTo/hfa.Brokers.Messages.csproj hfa.Notification.DTo/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/SyncLibrary
RUN dotnet build -c Release -o /app -r linux-$arch

ARG arch=x64
FROM build AS publish
ARG arch
RUN dotnet publish -c Release -o /app -r linux-$arch

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "hfa.Synker.batch.dll"]
