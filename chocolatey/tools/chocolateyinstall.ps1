$ErrorActionPreference = 'Stop';

$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = 'https://github.com/YoshihiroIto/JitPad/releases/download/1.0.10/JitPad.1.0.10.zip'

$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  url           = $url
  softwareName  = 'JitPad*'
  checksum      = 'A97AF2D3DA48BAFBD1E46B3E84410C1F89573BBCE9764041624B5C566CFA07B6'
  checksumType  = 'sha256'
}

Install-ChocolateyZipPackage @packageArgs
