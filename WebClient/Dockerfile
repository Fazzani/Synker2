FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.1 AS build
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
