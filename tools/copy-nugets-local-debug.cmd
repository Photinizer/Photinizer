@echo off
echo copy Photinizer.Cli
copy /Y ".\..\src\Photinizer.Cli\bin\Debug\Photinizer.Cli.0.1.0.nupkg" "c:\LocalNuGetPackages\"
echo copy Photinizer.Build.Own
copy /Y ".\..\src\Photinizer.Build.Own\bin\Debug\Photinizer.Build.Own.0.1.0.nupkg" "c:\LocalNuGetPackages\"
pause