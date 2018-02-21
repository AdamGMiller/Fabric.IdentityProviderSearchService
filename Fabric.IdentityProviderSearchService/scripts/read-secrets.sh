#!/bin/bash

sourceFile=$1

echo $sourceFile

clientSecretJson=$(<$sourceFile)
echo $clientSecretJson

installersecret=$(echo $clientSecretJson | jq -r .installerSecret) 

echo $installersecret

echo "##vso[task.setvariable variable=FABRIC_INSTALLER_SECRET;]$installersecret"
