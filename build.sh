#!/usr/bin/env bash

set -evuxo

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then  
  rm -R $artifactsFolder
fi

dockerImageBase="synker/broker"
dockerImage=$dockerImageBase:$TRAVIS_BUILD_NUMBER-$arch
echo $dockerImage

if [ $DOCKER_BUILD="true" ]; then
  echo "Is Docker build";
  docker build --build-arg arch=$arch -t $dockerImage -t $dockerImageBase:latest .
  docker images
  docker run --rm $dockerImage
  docker login -u=$DOCKER_USER -p=$DOCKER_PASS
  docker push $dockerImage
  exit 0
fi

cd WebClient
npm install
cd .. dotnet restore 

dotnet build -c Release -o ./artifacts

echo "Running Tests"

dotnet test hfa.synker.batch.test/hfa.synker.batch.test.csproj
dotnet test hfa.tvhLibrary.test/hfa.tvhLibrary.test.csproj

exit 0