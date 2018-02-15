function ReplaceStrings ($stringToFind, $stringToReplace) {
    $markdownFiles = Get-ChildItem . *.md
    foreach ($file in $markdownFiles)
    {
        (Get-Content $file.PSPath) |
        Foreach-Object { $_ -replace $stringToFind, $stringToReplace } |
        Set-Content $file.PSPath
    }
}

Write-Host "changing to markdown directory"
Set-Location "$2"

Write-Host "Cloning Git Wiki repo..."
$REPO="https://$1@github.com/HealthCatalyst/Fabric.IdentityProviderSearchService.wiki.git"
git clone $REPO

Write-Host "renaming MD files"
Move-Item overview.md API-Reference-Overview.md 
Move-Item paths.md API-Reference-Resources.md
Move-Item definitions.md API-Reference-Models.md
Move-Item security.md API-Reference-Security.md

Write-Host "replacing old file names with new ones"
ReplaceStrings 'overview.md' 'API-Reference-Overview'
ReplaceStrings 'paths.md' 'API-Reference-Resources'
ReplaceStrings 'definitions.md' 'API-Reference-Models'
ReplaceStrings 'security.md' 'API-Reference-Security'

Write-Host "moving files to Fabric.IdentityProviderSearchService.wiki"
Get-ChildItem -Path "*.md" | Move-Item -Destination "Fabric.IdentityProviderSearchService.wiki" -Force

Write-Host "Changing directory..."
Set-Location Fabric.IdentityProviderSearchService.wiki

git config user.name "vsts build"
git config user.email "kyle.paul@healthcatalyst.com"
git add *.md

Write-Host "committing files"
git commit -m 'update API documentation'
Write-Host "pushing files to github"
git push origin master

