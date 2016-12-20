'Build starting'

$toolsDir = "tools"
$nuget = "$toolsDir\nuget.exe"

. $nuget "Install" "FAKE.Core" "-OutputDirectory" $toolsDir "-ExcludeVersion"
. $nuget "Install" "xunit.runner.console" "-OutputDirectory" $toolsDir "-ExcludeVersion"

'Dependencies downloaded, time to run FAKE'
. "$toolsDir\FAKE.Core\tools\Fake.exe" "build.fsx"
