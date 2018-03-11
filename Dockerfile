ARG arch=x64
FROM microsoft/aspnetcore:2.0 AS base
ARG arch
LABEL maintainer="synker-team@synker.ovh" \
      description="Synker suite application" \
      system.dist="linux" system.arch="$arch" multi.name="Synker notification broker"
RUN apt-get -qq update && apt-get -y upgrade
# Create Group and user
RUN groupadd -r synker && useradd --no-log-init -r -g synker synker
USER synker:synker
# add user and group permissions
WORKDIR /home/synker
