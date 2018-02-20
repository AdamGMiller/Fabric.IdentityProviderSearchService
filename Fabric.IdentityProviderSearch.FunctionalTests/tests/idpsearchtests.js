//get fabric installer secret from environment variable  (build pipeline will create an identity container same as it does in auth release pipeline )

//register the idp search api with identity

//create a client that has the fabric\identity.search scope 

//create tests for finding users, groups, and both 

var chakram = require("chakram");
var expect = require("chakram").expect;

describe("identity provider search tests", function(){    
    var fabricInstallerSecret = process.env.FABRIC_INSTALLER_SECRET;    
    var baseIdPSearchUrl = process.env.BASE_IDP_SEARCH_URL;
    var baseIdentityUrl = process.env.BASE_IDENTITY_URL;        

    if(!baseIdPSearchUrl) {
        baseIdPSearchUrl = "http://localhost:5009";
    }

    if (!baseIdentityUrl) {
        baseIdentityUrl = "http://localhost:5001";
    }    

    var authRequestOptions = {
        headers: {
            "Content-Type": "application/json",
            "Accept": "application/json",
            "Authorization": ""
        }
    }

    var searchClientFuncTest = {
        "clientId": "search-func-test",
        "clientName": "Search Functional Test Client",
        "requireConsent": "false",
        "allowedGrantTypes": ["client_credentials", "password"],
        "allowedScopes": [
            "fabric/identity.searchusers",
        ]
    }

    function getAccessToken(clientData) {
        return chakram.post(baseIdentityUrl + "/connect/token", undefined, clientData)
            .then(function (postResponse) {
                var accessToken = "Bearer " + postResponse.body.access_token;
                return accessToken;
            });
    }

    function getAccessTokenForInstaller(installerClientSecret) {
        var postData = {
            form: {
                "client_id": "fabric-installer",
                "client_secret": installerClientSecret,
                "grant_type": "client_credentials",
                "scope": "fabric/identity.manageresources fabric/authorization.manageclients"
            }
        };        

        return getAccessToken(postData);
    }

    function getAccessTokenForSearchClient(secret) {
        var clientData = {
            form: {
                "client_id": "search-func-test",
                "client_secret": secret,
                "grant_type": "client_credentials",
                "scope": "fabric/identity.searchusers"
            }
        }

        return getAccessToken(clientData);
    }

    var idpSearchApi = {
        "name": "idpsearch-api",
        "userClaims": [
            "name",
            "email",
            "role",
            "groups"
        ],
        "scopes": [{ "name": "idpsearch-api", "displayName": "Identity Provider Search API" }]
    }

    function registerSearchApiWithIdentity() {
        return chakram.post(baseIdentityUrl + "/api/apiresource", idpSearchApi, authRequestOptions)
    }

    function registerSearchClientWithIdentity(){
        return chakram.post(baseIdentityUrl + "/api/client", searchClientFuncTest, authRequestOptions)
                .then(function (clientResponse) {
                    expect(clientResponse).to.have.status(201);
                    expect(clientResponse).to.comprise.of.json({ clientId: "search-func-test" });
                    return clientResponse.body.clientSecret;
                });
    }

    function bootstrapIdentityServer() {        
        return getAccessTokenForInstaller(fabricInstallerSecret)
            .then(function (retrievedAccessToken) {
                authRequestOptions.headers.Authorization = retrievedAccessToken;
            })
            .then(registerSearchApiWithIdentity)
            .then(registerSearchClientWithIdentity)
            .then(function(searchClientSecret){
                return getAccessTokenForSearchClient(searchClientSecret);
            })
            .then(function(searchClientAccessToken){
                authRequestOptions.headers.Authorization = searchClientAccessToken;
            });
    }

    before("running before", function () {
        this.timeout(5000);
        return bootstrapIdentityServer();
    });

    describe("search for users and groups", function(){
        it("should find both users and groups", function(){
            chakram.get(baseIdPSearchUrl + "/principals/search?searchtext=ky", authRequestOptions)
                .then(function(searchResponse){
                    expect(clientResponse).to.have.status(200);
                    expect(clientResponse).to.comprise.of.json({ clientId: "func-test" });                    
                });
        })
    });
});