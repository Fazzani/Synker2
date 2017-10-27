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
curl --insecure -o commands.json -X GET -H "Content-type:application/json" -H "Authorization:Bearer $bearer" $hostApi/command/users/1?all=true
readarray -t tab < <(jq -r '.[].commandText' commands.json)
readarray -t tabIds < <(jq '.[].id' commands.json)
#echo ${tab[@]}
#echo ${tabCmds[@]}
cpt=0
for i in "${tab[@]}"
do
  echo "${date} Executing $i"
  eval $i
  echo "Put command treated ${tabIds[$cpt]}"
  curl -v --insecure -X PUT -H "Content-type:application/json" -H "Content-Length:0" -H "Authorization:Bearer $bearer" $hostApi/command/treat/${tabIds[$cpt]}
  let "cpt++"
done

quit 0;
