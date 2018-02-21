#!/bin/bash

installerSecretString=$1
destinationFile=$3

json="{\"installerSecret\":\""$installerSecretString"\"}"  

echo -e $json
echo $destinationFile
echo $json > $destinationFile