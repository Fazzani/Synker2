ARG arch=x64
FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
ARG arch
LABEL maintainer="synker-team@synker.ovh" \
      description="WebApi Synker suite application" \
      system.dist="linux" system.arch="$arch" multi.name="Synker WebClient"
RUN apt-get -qq update && apt-get -y upgrade
# Create Group and user
#RUN groupadd -r synker && mkdir /home/synker && useradd --no-log-init -d /home/synker -r -g synker synker
#RUN mkdir /app && chown synker:synker -R /home/synker && chmod -R 777 /home/synker
#USER synker:synker
# add user and group permissions
WORKDIR /app
#Expose Smtp and POP ports for mailing
EXPOSE 56800
HEALTHCHECK --interval=3m --timeout=1m CMD curl -f http://localhost:56800/api/v1/HealthCheck/webApi || exit 1

ARG arch=x64
FROM microsoft/dotnet:2.1-sdk AS build
ARG arch
WORKDIR /src
#COPY Synker.sln ./
#COPY WebApi/hfa.WebApi.csproj WebApi/
#COPY hfa.Synker.Service/hfa.Synker.Service.csproj hfa.Synker.Service/
#COPY PlaylistBaseLibrary/hfa.PlaylistBaseLibrary.csproj PlaylistBaseLibrary/
#COPY Brokers/hfa.Notification.DTo/hfa.Brokers.Messages.csproj Brokers/hfa.Notification.DTo/
#COPY SyncLibrary/hfa.Synker.batch.csproj SyncLibrary/
#COPY hfa.TvhLibrary/hfa.TvhLibrary.csproj hfa.TvhLibrary/
COPY . .
# TODO : TO Remove after updating 2.1.3 dotnet cli version
#RUN dotnet sln Synker.sln remove docker-compose.dcproj
RUN dotnet restore -nowarn:msb3202,nu1503 Synker.sln
WORKDIR /src/WebApi
RUN dotnet build --no-restore -c Release -o /app -r linux-$arch

ARG arch=x64
FROM build AS publish
ARG arch
RUN dotnet publish -c Release -o /app -r linux-$arch

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN mkdir /app/.well-known
ENTRYPOINT ["dotnet", "hfa.WebApi.dll"]
