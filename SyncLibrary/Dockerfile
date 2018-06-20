ARG arch=x64
ARG version=1.0.0
FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
ARG arch
ARG version
ENV VERSION=$version
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
FROM microsoft/dotnet:2.1.300-sdk AS build
ARG arch
WORKDIR /src
COPY . .
RUN dotnet restore -nowarn:msb3202,nu1503 ./Synker.sln
WORKDIR /src/SyncLibrary
RUN dotnet build --no-restore  -c Release -o /app #-r linux-$arch

ARG arch=x64
FROM build AS publish
ARG arch
RUN dotnet publish -c Release -o /app -r linux-$arch

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "hfa.Synker.batch.dll"]
