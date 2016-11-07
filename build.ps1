$toolsDir = "tools"
$nuget = "$toolsDir\nuget.exe"

. $nuget "Install" "FAKE.Core" "-OutputDirectory" $toolsDir "-ExcludeVersion"
. $nuget "Install" "xunit.runner.console" "-OutputDirectory" $toolsDir "-ExcludeVersion"

. "$toolsDir\FAKE.Core\tools\Fake.exe" "build.fsx"