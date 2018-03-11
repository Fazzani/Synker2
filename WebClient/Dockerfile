FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
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
RUN dotnet build -c Release -o /app

FROM build AS publish
#RUN apt-get -qq update && apt-get -qqy --no-install-recommends install wget gnupg \
    #git \
    #unzip
#RUN curl -sL https://deb.nodesource.com/setup_8.x | bash -
#RUN apt-get install -y nodejs
#RUN rm -rf node_modules && rm -rf /usr/lib/node_modules 
#&& npm install
#RUN npm i isarray && npm i --save-dev core-util-is && npm i webpack@latest -g
#RUN apt-get -qq update && apt-get -y upgrade
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "hfa.WebClient.dll"]
