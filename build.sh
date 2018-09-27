#!/usr/bin/env bash

#Notif another build
trigger(){

  if [ -f ./trigger-build.sh ]; then
    . ./trigger-build.sh $1 $2
  fi
}

build_only_project()
{
  version=$1
  project=$2
  dockerfilePath=$3

  docker build -t "synker/${project}:${version}" -t "synker/${project}:latest" --build-arg version=$version -f $dockerfilePath .
  docker push "synker/${project}"
  trigger "Fazzani/synker-docker"
}

exit 0