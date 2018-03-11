ARG arch=x64
FROM microsoft/aspnetcore:2.0 AS base
ARG arch
LABEL maintainer="synker-team@synker.ovh" \
      description="WebClient Synker suite application" \
      system.dist="linux" system.arch="$arch" multi.name="Synker WebClient"
RUN apt-get -qq update && apt-get -y upgrade
# Create Group and user
# add user and group permissions
RUN groupadd -r synker && mkdir /home/synker && useradd --no-log-init -d /home/synker -r -g synker synker
RUN chown synker:synker -R /home/synker
USER synker:synker
RUN chmod -R 700 /home/synker/webclient
WORKDIR /home/synker/webclient
#Expose Smtp and POP ports for mailing
EXPOSE 80


ARG arch=x64
FROM microsoft/aspnetcore-build:2.0 AS build
ARG arch
ENV NODE_VERSION 8.9.4
ENV NODE_DOWNLOAD_URL https://nodejs.org/dist/v$NODE_VERSION/node-v$NODE_VERSION-linux-x64.tar.gz
ENV NODE_DOWNLOAD_SHA 21fb4690e349f82d708ae766def01d7fec1b085ce1f5ab30d9bda8ee126ca8fc

RUN curl -SL "$NODE_DOWNLOAD_URL" --output nodejs.tar.gz \
    && echo "$NODE_DOWNLOAD_SHA nodejs.tar.gz" | sha256sum -c - \
    && mkdir -p /usr/local/opt/nodejs8 \
    && tar -xzf "nodejs.tar.gz" -C /usr/local/opt/nodejs8 --strip-components=1 \
    && rm nodejs.tar.gz \
    && ln -f -s /usr/local/opt/nodejs8/bin/node /usr/local/bin/node \
    && ln -f -s /usr/local/opt/nodejs8/bin/node /usr/local/bin/nodejs

WORKDIR /src
COPY Synker.sln ./
COPY WebClient/hfa.WebClient.csproj WebClient/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/WebClient
RUN dotnet build -c Release -o /home/synker/webclient -r linux-$arch

ARG arch=x64
FROM build AS publish
ARG arch
#RUN apt-get -qq update && apt-get -qqy --no-install-recommends install wget gnupg \
    #git \
    #unzip
#RUN curl -sL https://deb.nodesource.com/setup_8.x | bash -
RUN dotnet publish -c Release -o /home/synker/webclient -r linux-$arch

FROM base AS final
WORKDIR /home/synker/webclient
COPY --from=publish /home/synker/webclient .
ENTRYPOINT ["dotnet", "hfa.WebClient.dll"]
