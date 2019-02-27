# require 3.0

param(
    # What do you want to run
    [Parameter(Mandatory=$true)]
    [string]
    [ValidateSet('Setup', 'Build', 'UnitTests', 'IntegrationTests', 'Package')]
    $Target
)

$fakeDir = ".fake"

if ($Target -eq 'Setup') {
    dotnet tool install fake-cli --tool-path ./$fakeDir
} else {
    . "$fakeDir/fake.exe" run ./build.fsx target $Target
}
