#!/usr/bin/env bash

#Notif another build
trigger(){

  if [ -f ./trigger-build.sh ]; then
    . ./trigger-build.sh $1
  fi

}

set -evuxo
echo "$TRAVIS_TAG"
if [ -z "$TRAVIS_TAG" ]; then
  echo "Isn't a Docker build";
  exit 0
fi

if [[ "$DOCKER_BUILD" == true ]]; then
  echo "Is a Docker build";
  docker login -u=$DOCKER_USER -p=$DOCKER_PASS
  export version=$TRAVIS_TAG
  
  # si le message contient build_webclient alors on build uniquement le client
  if [[ $TRAVIS_COMMIT_MESSAGE == *"build_webclient"* ]]; then
    docker build -t synker/webclient:${version} -t synker/webclient:latest -f WebClient/Dockerfile .
	docker push synker/webclient
    trigger "Fazzani/synker-docker"
	exit 0
  fi

  # si le message contient build_webapi alors on build uniquement l'api
  if [[ $TRAVIS_COMMIT_MESSAGE == *"build_webapi"* ]]; then
    docker build -t synker/webapi:${version} -t synker/webapi:latest -f WebApi/Dockerfile .
	docker push synker/webapi
    trigger "Fazzani/synker-docker"
	exit 0
  fi

  # si le message contient build_webapi alors on build uniquement l'api
  if [[ $TRAVIS_COMMIT_MESSAGE == *"build_batch"* ]]; then
    docker build -t synker/batch:${version} -t synker/batch:latest -f SyncLibrary/Dockerfile .
	docker push synker/batch
    trigger "Fazzani/synker-docker"
	exit 0
  fi

  # build image with github tag version
  docker-compose build
  docker-compose push

  # build image with latest tag version
  export version="latest"
  docker-compose build
  docker-compose push
  
  trigger "Fazzani/synker-docker"
  
  exit 0
fi

exit 0