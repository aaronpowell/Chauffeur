# require 3.0

param(
    # What do you want to run
    [Parameter(Mandatory=$true)]
    [string]
    [ValidateSet('Setup', 'Build', 'UnitTests', 'IntegrationTests', 'Package')]
    $Target
)

$toolsDir = "tools"

if ($Target -eq 'Setup') {
    $nuget = "$toolsDir\nuget.exe"

    . $nuget "Install" "FAKE.Core" "-OutputDirectory" $toolsDir "-ExcludeVersion" -Version 5.3.0
    . $nuget "Install" "FAKE.Core.Environment" "-OutputDirectory" $toolsDir "-ExcludeVersion" -Version 5.5.0
    . $nuget "Install" "FAKE.DotNet.AssemblyInfoFile" "-OutputDirectory" $toolsDir "-ExcludeVersion" -Version 5.5.0
    . $nuget "Install" "Fake.IO.FileSystem" "-OutputDirectory" $toolsDir "-ExcludeVersion" -Version 5.5.0
    . $nuget "Install" "Fake.DotNet.Testing.XUnit2" "-OutputDirectory" $toolsDir "-ExcludeVersion" -Version 5.5.0
    . $nuget "Install" "xunit.runner.console" "-OutputDirectory" $toolsDir "-ExcludeVersion" -Version 2.2.0
    . $nuget "Install" "FSharpLint.Fake" "-OutputDirectory" $toolsDir "-ExcludeVersion" -Version 0.7.6
} else {
    . "$toolsDir\FAKE.Core\tools\FAKE.exe" "build.fsx" "target=$Target" "--removeLegacyFakeWarning"
}
