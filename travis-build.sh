#!/usr/bin/env bash

set -evuxo
echo "$TRAVIS_TAG"
if [ -z "$TRAVIS_TAG" ]; then
  echo "Isn't a Docker build";
  exit 0
fi

if [[ "$DOCKER_BUILD" == true ]]; then
  echo "Is a Docker build";
  docker login -u=$DOCKER_USER -p=$DOCKER_PASS

  # build image with github tag version
  export version=$TRAVIS_TAG
  docker-compose build
  docker-compose push

  # build image with latest tag version
  export version="latest"
  docker-compose build
  docker-compose push
  
  if [ -f ./trigger-build.sh ]; then
    . ./trigger-build.sh "Fazzani/synker-docker"
  fi
  exit 0
fi

exit 0