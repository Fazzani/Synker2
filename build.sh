#!/usr/bin/env bash

set -evuxo

artifactsFolder="./artifacts"

if [ $DOCKER_BUILD="true" ]
  echo "Is Docker build";
fi

if [ -d $artifactsFolder ]; then  
  rm -R $artifactsFolder
fi

cd WebClient
npm install
cd .. dotnet restore 

dotnet build -c Release -o ./artifacts

echo "Running Tests"

dotnet test hfa.synker.batch.test/hfa.synker.batch.test.csproj
dotnet test hfa.tvhLibrary.test/hfa.tvhLibrary.test.csproj

exit 0