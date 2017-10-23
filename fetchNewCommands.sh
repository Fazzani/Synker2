#!/bin/bash

#/**
# * Récupérer une liste de commandes et les executer depuis l'api
# */
function quit {
    exit $1;
}

hostApi='http://0.0.0.0:56800/api/v1'
echo  $hostApi

echo "Getting auth token.."
curl --insecure -o bearer.json -X POST -H "Content-type: application/json" -d '{"password": "batch2017", "username": "support@synker.ovh"}' $hostApi/auth/token > bearer.json
bearer=($(cat bearer.json | jq '.accessToken'))
#echo "Put token to the file $bearer bearer.json"
echo "Get commands"
curl -v --insecure -o commands.json -X GET -H "Content-type:application/json" $hostApi/command?all=true
v1=($(cat commands.json | jq '.'))
echo $v1
quit 0;
