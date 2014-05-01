param($Publish)

$toolsDir = "tools"
$nuget = "$toolsDir\nuget.exe"

if (!(Test-Path $toolsDir)) {
    New-Item $toolsDir -ItemType "Directory"
}

if (!(Test-Path $nuget)) {
    "NuGet not installed, installing"

    $wc = New-Object System.Net.WebClient
    $wc.DownloadFile("http://nuget.org/nuget.exe", "$pwd\$nuget")
}

. $nuget "Install" "FAKE" "-OutputDirectory" "tools" "-ExcludeVersion"
. $nuget "Install" "nunit.runners" "-OutputDirectory" "tools" "-ExcludeVersion"

. "$toolsDir\FAKE\tools\Fake.exe" "build.fsx"