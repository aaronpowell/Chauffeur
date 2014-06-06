$toolsDir = "tools"
$nuget = "$toolsDir\nuget.exe"

. $nuget "Install" "FAKE" "-OutputDirectory" $toolsDir "-ExcludeVersion"
. $nuget "Install" "nunit.runners" "-OutputDirectory" $toolsDir "-ExcludeVersion"

. "$toolsDir\FAKE\tools\Fake.exe" "build.fsx"