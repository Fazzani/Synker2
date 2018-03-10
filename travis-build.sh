#!/usr/bin/env bash

set -evuxo
echo "$TRAVIS_TAG"

if [[ "$DOCKER_BUILD" == true && "$TRAVIS_TAG"!="" ]]; then
  echo "Is a Docker build";
  docker login -u=$DOCKER_USER -p=$DOCKER_PASS
  version=${TRAVIS_TAG:latest}
  export $version
  docker-compose build
  docker-compose push
  exit 0
fi

echo "Isn't a Docker build";

exit 0