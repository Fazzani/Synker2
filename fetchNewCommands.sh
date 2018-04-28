#!/bin/bash

#/**
# * Récupérer une liste de commandes et les executer depuis l'api
# */
function quit {
    exit $1;
}

hostApi='http://api.synker.ovh/api/v1'

#
# @desc updating command status
# @params bearer commandId status
#
function putCmdStatus {
     curl -v --insecure -X PUT -H "Content-type:application/json" -H "Content-Length:0" -H "Authorization:Bearer $1" $hostApi/command/$2/status/$3
}

# TODO : Secure password (Get from file or env  var)
echo "Getting auth token.."
curl --insecure -o bearer.json -X POST -H "Content-type: application/json"  -d '{"password": "batch2017", "username": "support@synker.ovh"}' $hostApi/auth/token
bearer=($(cat bearer.json | jq -r '.accessToken'))
#echo "Put token to the file $bearer bearer.json"
echo "Getting commands"
curl --insecure -o commands.json -X GET -H "Content-type:application/json" -H "Authorization:Bearer $bearer" $hostApi/command/users/1?all=true
readarray -t tab < <(jq -r '.[].commandText' commands.json)
readarray -t tabIds < <(jq '.[].id' commands.json)
#echo ${tab[@]}
#echo ${tabCmds[@]}
cpt=0
for i in "${tab[@]}"
do
  echo 'Flag Command ${tabIds[$cpt]} as treating'
  putCmdStatus $bearer ${tabIds[$cpt]} 1;
  echo "${date} Executing $i"
  eval $i
  echo 'Flag Command ${tabIds[$cpt]} as treated'
  putCmdStatus $bearer ${tabIds[$cpt]} 2;
  let "cpt++"
done

quit 0;
