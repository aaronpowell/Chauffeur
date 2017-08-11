.\build.ps1 -Target Setup
.\build.ps1 -Target Build

Push-Location .\Chauffeur.Demo\bin
./chauffeur.runner.exe delivery "-p:pwd=password!1"
Pop-Location

$iisExpress = "${env:ProgramFiles(x86)}\IIS Express\iisexpress.exe"

& $iisExpress -path:"$PSScriptRoot\Chauffeur.Demo"
