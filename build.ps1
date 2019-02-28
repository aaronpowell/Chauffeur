# require 3.0

param(
    # What do you want to run
    [Parameter(Mandatory=$true)]
    [string]
    [ValidateSet('Setup', 'Build', 'Test', 'Package')]
    $Target
)

$fakeDir = ".fake"

if ($Target -eq 'Setup') {
    dotnet tool install fake-cli --tool-path ./$fakeDir
    dotnet tool install coverlet.console --tool-path ./$fakeDir

    (New-Object System.Net.WebClient).DownloadFile("https://github.com/codecov/codecov-exe/releases/download/1.2.0/Codecov.zip", (Join-Path $pwd "Codecov.zip")) # Download Codecov.zip from github release.
    Expand-Archive .\Codecov.zip -DestinationPath ./codecov/$fakeDir
    Remove-Item .\Codecov.zip
} else {
    . "$fakeDir/fake.exe" run ./build.fsx target $Target
}
