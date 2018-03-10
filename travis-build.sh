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
  version=$TRAVIS_TAG
  export $version
  docker-compose build

  # build image with latest tag version
  version="latest"
  export $version

  docker-compose push
  exit 0
fi

exit 0