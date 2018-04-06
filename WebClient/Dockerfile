ARG arch=x64
FROM microsoft/aspnetcore:2.0 AS base
ARG arch
LABEL maintainer="synker-team@synker.ovh" \
      description="WebClient Synker suite application" \
      system.dist="linux" system.arch="$arch" multi.name="Synker WebClient"
RUN apt-get -qq update && apt-get -y upgrade
# Create Group and user
# add user and group permissions
#RUN groupadd -r synker && mkdir /home/synker && useradd --no-log-init -d /home/synker -r -g synker synker
#RUN mkdir /app && chown synker:synker -R /home/synker && chmod -R 777 /home/synker
#USER synker:synker
#RUN chmod -R 700 /app
#WORKDIR /app
EXPOSE 56801
HEALTHCHECK --interval=5m --timeout=3s CMD curl -f http://localhost:58601/ || exit 1


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

# Install yarn
RUN apt-get update && apt-get install -y apt-transport-https
RUN curl -sS https://dl.yarnpkg.com/debian/pubkey.gpg | apt-key add -
RUN echo "deb https://dl.yarnpkg.com/debian/ stable main" | tee /etc/apt/sources.list.d/yarn.list
RUN apt-get update && apt-get install -y yarn

WORKDIR /src
COPY Synker.sln ./
COPY . .
#COPY WebClient/hfa.WebClient.csproj WebClient/
RUN dotnet sln Synker.sln remove docker-compose.dcproj
RUN dotnet restore -nowarn:msb3202,nu1503 Synker.sln
WORKDIR /src/WebClient
RUN dotnet build -c Release -o /app -r linux-$arch 

ARG arch=x64
FROM build AS publish
ARG arch
RUN npm install && ls -la
RUN dotnet publish -c Release -o /app -r linux-$arch

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN mkdir /app/.well-known
ENTRYPOINT ["dotnet", "hfa.WebClient.dll"]
