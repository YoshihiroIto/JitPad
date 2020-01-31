[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

# Output directory
if (Test-Path tools)
{
    Remove-Item -path tools -recurse -force
}
New-Item -name tools -type directory

# --[GenLibraryList]--------------------------------------------------------------
Invoke-WebRequest -Uri https://github.com/YoshihiroIto/GenLibraryList/releases/download/v1.2/GenLibraryList.zip -OutFile GenLibraryList.zip
Expand-Archive -Path GenLibraryList.zip -DestinationPath tools\GenLibraryList
Remove-Item GenLibraryList.zip

