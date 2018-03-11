ARG arch=x64
FROM microsoft/aspnetcore:2.0 AS base
ARG arch
LABEL maintainer="synker-team@synker.ovh" \
      description="WebApi Synker suite application" \
      system.dist="linux" system.arch="$arch" multi.name="Synker WebClient"
RUN apt-get -qq update && apt-get -y upgrade
# Create Group and user
RUN groupadd -r synker && mkdir /home/synker && useradd --no-log-init -d /home/synker -r -g synker synker
RUN mkdir /home/synker/webapi && chown synker:synker -R /home/synker && chmod -R 777 /home/synker
USER synker:synker
# add user and group permissions
WORKDIR /home/synker/webapi
#Expose Smtp and POP ports for mailing
EXPOSE 80

ARG arch=x64
FROM microsoft/aspnetcore-build:2.0 AS build
ARG arch
WORKDIR /src
COPY Synker.sln ./
COPY WebApi/hfa.WebApi.csproj WebApi/
COPY hfa.Synker.Service/hfa.Synker.Service.csproj hfa.Synker.Service/
COPY PlaylistBaseLibrary/hfa.PlaylistBaseLibrary.csproj PlaylistBaseLibrary/
COPY Brokers/hfa.Notification.DTo/hfa.Brokers.Messages.csproj Brokers/hfa.Notification.DTo/
COPY SyncLibrary/hfa.Synker.batch.csproj SyncLibrary/
COPY hfa.TvhLibrary/hfa.TvhLibrary.csproj hfa.TvhLibrary/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/WebApi
RUN dotnet build -c Release -o /home/synker/webapi -r linux-$arch

ARG arch=x64
FROM build AS publish
ARG arch
RUN dotnet publish -c Release -o /home/synker/webapi -r linux-$arch

FROM base AS final
WORKDIR /home/synker/webapi
COPY --from=publish /home/synker/webapi .
ENTRYPOINT ["dotnet", "hfa.WebApi.dll"]
