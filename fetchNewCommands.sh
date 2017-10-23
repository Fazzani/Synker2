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
curl --insecure -o bearer.json -X POST -H "Content-type: application/json"  -d '{"password": "batch2017", "username": "support@synker.ovh"}' $hostApi/auth/token
bearer=($(cat bearer.json | jq -r '.accessToken'))
#echo "Put token to the file $bearer bearer.json"
echo "Get commands"
curl -v --insecure -o commands.json -X GET -H "Content-type:application/json" -H "Authorization:Bearer $bearer"  $hostApi/command/users/1?all=true
#cat commands.json
v1=($(jq -rc '.[]."commandText"' commands.json))
#echo $v1
#printf '%s\n' "${v1[@]}"
cat commands.json | jq -r '.[].commandText'
#TODO: Mettre la liste des commandes récupérer dans un array dans le bon ordre et les exécuter 
quit 0;
