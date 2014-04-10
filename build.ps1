param($Publish)

$msbuild = "${env:ProgramFiles(x86)}\MSBuild\12.0\Bin\msbuild.exe"

. $msbuild Chauffeur.sln /p:Configuration=Release /t:"Clean;Build"

rm *.nupkg

cd Chauffeur
nuget pack -OutputDirectory ../ -Prop Configuration=Release
nuget pack -OutputDirectory ../ -Prop Configuration=Release -symbols

cd ../Chauffeur.Runner
nuget pack -OutputDirectory ../ -Prop Configuration=Release
nuget pack -OutputDirectory ../ -Prop Configuration=Release -symbols

cd ..

if ($Publish) {
    gci *.nupkg | %{ nuget push $_.FullName }
}