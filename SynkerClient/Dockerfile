ARG arch=x64
ARG version=1.0.0
FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
ARG arch
ARG version
ENV VERSION=$version
LABEL maintainer="synker-team@synker.ovh" \
  description="WebClient Synker suite application" \
  system.dist="linux" system.arch="$arch" multi.name="Synker WebClient"
RUN apt-get -qq update && apt-get -y upgrade
EXPOSE 56801
HEALTHCHECK --interval=3m --timeout=1m CMD curl -f http://localhost:56801/liveness || exit 1

ARG arch=x64
FROM microsoft/dotnet:2.1-sdk AS publish
ARG arch
ENV NODE_VERSION 8.11.1
ENV NODE_DOWNLOAD_SHA 0e20787e2eda4cc31336d8327556ebc7417e8ee0a6ba0de96a09b0ec2b841f60

RUN curl -SL "https://nodejs.org/dist/v${NODE_VERSION}/node-v${NODE_VERSION}-linux-x64.tar.gz" --output nodejs.tar.gz \
  && echo "$NODE_DOWNLOAD_SHA nodejs.tar.gz" | sha256sum -c - \
  && tar -xzf "nodejs.tar.gz" -C /usr/local --strip-components=1 \
  && rm nodejs.tar.gz \
  && ln -s /usr/local/bin/node /usr/local/bin/nodejs \
  && npm i -g @angular/cli

WORKDIR /src
# copy csproj and restore as distinct layers
COPY ./SynkerClient/*.csproj ./
COPY ./SynkerClient/ClientApp/package.json ./
RUN dotnet restore --verbosity quiet
RUN npm install 

# copy everything else and build
RUN mkdir ./SynkerClient \
    && mkdir ./PlaylistBaseLibrary
COPY ./SynkerClient ./SynkerClient
COPY ./PlaylistBaseLibrary ./PlaylistBaseLibrary
WORKDIR /src/SynkerClient
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SynkerClient.dll"]