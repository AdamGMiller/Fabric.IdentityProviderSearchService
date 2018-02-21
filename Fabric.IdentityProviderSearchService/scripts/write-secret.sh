#!/bin/bash

installerSecretString=$1
destinationFile=$2

json="{\"installerSecret\":\""$installerSecretString"\"}"  

echo -e $json
echo $destinationFile
echo $json > $destinationFile