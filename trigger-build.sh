#!/bin/bash

### ### ### ### ### ### ### ### ### ### ###
# trigger travis build by code
### ### ### ### ### ### ### ### ### ### ###

id="${1//\//%2F}"
echo $id

token=eldMP66rbB3lYO-pNF94AA
body='{
"request": {
"branch":"master"
}}'

curl -s -X POST \
   -H "Content-Type: application/json" \
   -H "Accept: application/json" \
   -H "Travis-API-Version: 3" \
   -H "Authorization: token $token" \
   -d "$body" \
   https://api.travis-ci.org/repo/$id/requests