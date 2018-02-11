#!/usr/bin/env bash

set -euxo

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then  
  rm -R $artifactsFolder
fi

cd WebClient
npm install
cd .. dotnet restore 
dotnet build -c Release -o ./artifacts