# require 3.0

param(
    # What do you want to run
    [Parameter(Mandatory=$true)]
    [string]
    [ValidateSet('Setup', 'Build', 'UnitTests', 'IntegrationTests', 'Package')]
    $Target
)

'Build starting'

$toolsDir = "tools"

if ($Target -eq 'Setup') {
    $nuget = "$toolsDir\nuget.exe"

    . $nuget "Install" "FAKE.Core" "-OutputDirectory" $toolsDir "-ExcludeVersion"
    . $nuget "Install" "xunit.runner.console" "-OutputDirectory" $toolsDir "-ExcludeVersion"
} else {
    . "$toolsDir\FAKE.Core\tools\Fake.exe" "build.fsx" "target=$Target"
}
