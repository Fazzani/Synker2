#!/bin/bash

set -o errexit # Exit on error

echo '@@@@@@@@@@@@@@@@@@ Deploy @@@@@@@@@@@@@@@@@'
git stash save 'Before deploy' # Stash all changes before deploy
git checkout deploy && git push --set-upstream origin deploy &&
git merge master --no-ff --no-edit # Merge in the master branch without prompting
#npm run build:dist # Generate the bundled Javascript and CSS
git push
#git push heroku deploy:master # Deploy 
git checkout master # Checkout master again
git stash pop # And restore the changes
git add --all && git commit -am "merge new version"
echo 'Merge to master'
version=`npm version patch`
echo "Pushing a new version : ${version}"
git push &&
git tag -a $version -m "Bumped version number to ${version}" && git push origin $version
echo '---------------------------   The end   -----------------------------------'
exit 0