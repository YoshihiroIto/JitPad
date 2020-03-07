$ErrorActionPreference = 'Stop';

$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = 'https://github.com/YoshihiroIto/JitPad/releases/download/1.0.9/JitPad.1.0.9.zip'

$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  url           = $url
  softwareName  = 'JitPad*'
  checksum      = '53EB0F60C9FB1174D796FC51911894303B93A08BBB06A52E9B8261D50EB7827D'
  checksumType  = 'sha256'
}

Install-ChocolateyZipPackage @packageArgs
