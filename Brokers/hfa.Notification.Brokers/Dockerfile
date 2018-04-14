ARG arch=x64
FROM microsoft/aspnetcore:2.0 AS base
ARG arch
LABEL maintainer="synker-team@synker.ovh" \
      description="Notification broker service for Synker suite application" \
      system.dist="linux" system.arch="$arch" multi.name="Synker notification broker"
RUN apt-get -qq update && apt-get -y upgrade
# Create Group and user
#RUN groupadd -r synker && useradd --no-log-init -r -g synker synker
#USER synker:synker
# add user and group permissions
WORKDIR /app
#Expose Smtp and POP ports for mailing
EXPOSE 110:110
EXPOSE 587:587

ARG arch=x64
FROM microsoft/aspnetcore-build:2.0 AS build
ARG arch
WORKDIR /src
COPY Synker.sln ./
COPY Brokers/hfa.Notification.Brokers/hfa.Notification.Brokers.csproj Brokers/hfa.Notification.Brokers/
COPY Brokers/hfa.Notification.DTo/hfa.Brokers.Messages.csproj Brokers/hfa.Notification.DTo/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/Brokers/hfa.Notification.Brokers
RUN dotnet build -c Release -o /app -r linux-$arch

FROM build AS publish
ARG arch
RUN dotnet publish -c Release -o /app -r linux-$arch

FROM base AS final
WORKDIR /happ
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "hfa.Notification.Brokers.dll"]
