param($Publish)

$msbuild = "${env:ProgramFiles(x86)}\MSBuild\12.0\Bin\msbuild.exe"

. $msbuild Chauffeur.sln /p:Configuration=Release /t:Rebuild

rm *.nupkg

cd Chauffeur
nuget pack -OutputDirectory ../
nuget pack -OutputDirectory ../ -symbols

cd ../Chauffeur.Runner
nuget pack -OutputDirectory ../
nuget pack -OutputDirectory ../ -symbols

cd ..

if ($Publish) {
    gci *.nupkg | %{ nuget push $_.FullName }
}