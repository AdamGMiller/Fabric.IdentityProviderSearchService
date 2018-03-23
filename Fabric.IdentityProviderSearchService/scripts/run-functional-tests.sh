#!/bin/bash

docker stop idpsearch-functional-identity
docker rm idpsearch-functional-identity
docker network rm idpsearch-functional-tests

docker pull healthcatalyst/fabric.identity

docker network create idpsearch-functional-tests

docker run -d --name idpsearch-functional-identity \
	-p 5001:5001 \
	-e "HostingOptions__StorageProvider=InMemory" \
	-e "HostingOptions__AllowUnsafeEval=true" \
	-e "IssuerUri=http://localhost:5001" \
	-e "IDENTITYSERVERCONFIDENTIALCLIENTSETTINGS__AUTHORITY=http://localhost:5001" \
	--network="idpsearch-functional-tests" \
	healthcatalyst/fabric.identity
echo "started identity"
sleep 3

output=$(curl -sSL https://raw.githubusercontent.com/HealthCatalyst/Fabric.Identity/master/Fabric.Identity.API/scripts/setup-samples.sh | sh /dev/stdin http://localhost:5001)
installerSecret=$(echo $output | grep -oP '(?<="installerSecret":")[^"]*')
export FABRIC_INSTALLER_SECRET=$installerSecret
echo $installerSecret
export BASE_IDP_SEARCH_URL=http://localhost:55655

read -p "Start the idpsearch service, then press enter to continue."

cd ../../Fabric.IdentityProviderSearch.FunctionalTests

npm install
npm test

docker stop idpsearch-functional-identity
docker rm idpsearch-functional-identity
docker network rm idpsearch-functional-tests