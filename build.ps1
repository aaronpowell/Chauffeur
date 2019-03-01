# require 3.0

param(
    # What do you want to run
    [Parameter(Mandatory=$true)]
    [string]
    [ValidateSet('Setup', 'Build', 'Test', 'Package', 'CreateVariables')]
    $Target
)

$fakeDir = ".fake"

if ($Target -eq 'Setup') {
    dotnet tool install fake-cli --tool-path ./$fakeDir
    dotnet tool install coverlet.console --tool-path ./$fakeDir

    (New-Object System.Net.WebClient).DownloadFile("https://github.com/codecov/codecov-exe/releases/download/1.2.0/Codecov.zip", (Join-Path $pwd "Codecov.zip")) # Download Codecov.zip from github release.
    Expand-Archive .\Codecov.zip -DestinationPath ./$fakeDir/codecov
    Remove-Item .\Codecov.zip
} elseif ($Target -eq 'CreateVariables') {
    Push-Location ./.packaging
    Get-ChildItem *.nupkg | Foreach-Object {
        $zip = "$_.zip"
        Copy-Item $_ $zip -Force
        $unpack = "$($_.Name)_unpack"
        Expand-Archive $zip $unpack

        $nuspec = Get-ChildItem -Path $unpack -Filter *.nuspec

        [xml]$xml = Get-Content $nuspec.FullName

        $name = $xml.package.metadata.id
        $version = $xml.package.metadata.version
        $releaseNotes = $xml.package.metadata.releaseNotes

        Write-Host "##vso[task.setvariable variable=$($name)_version]$version"
        Write-Host "##vso[task.setvariable variable=$($name)_releaseNotes]$releaseNotes"

        Remove-Item $unpack -Recurse -Force
        Remove-Item $zip -Force
    }

    Pop-Location
} else {
    . "$fakeDir/fake.exe" run ./build.fsx target $Target
}
