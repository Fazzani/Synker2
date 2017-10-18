#!/bin/bash

#/**
# * Webgrab à la volée un xmltv
# * à partir d'un WebGrabConfig.xml passé en param
# */

#backup the current working dir
WG_BCKP_DIR="$(pwd)"

function quit {
    #restore previous working dir
    cd "$WG_BCKP_DIR"
    exit $1;
}

echo "Check if mono can be found"
which mono >/dev/null 2>&1 || { echo >&2 "Mono required, but it's not installed."; quit 1; }

echo "Create new temp working folder"
workingDir=`hexdump -n 16 -v -e '/1 "%02X"' -e '/16 "\n"' /dev/urandom`
echo "working dir : "$workingDir
mkdir  $workingDir
cd  $workingDir
pwd
echo "Download  $xml config from url $1"
wget -O  WebGrab++.config.xml $1
echo "Copy site pack to working directory"
cp -R ../siteini.pack .
echo "Run WebGrab"
path="`pwd`/WebGrab++.config.xml"
echo $path
pwd=`pwd`
mono ../bin/WebGrab+Plus.exe $pwd
echo "Moving the resulted xmltv file to FileBeated Folder"
mkdir ../FileBeatFolder

mv *.xmltv ../FileBeatFolder/
echo "Removing Working Directory"
cd .. && rm -R $workingDir

quit 0;

