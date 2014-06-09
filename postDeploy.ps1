$toolsDir = "tools"
$nuget = "$toolsDir\nuget.exe"

$symbolServer = "http://nuget.gw.symbolsource.org/Public/NuGet"

gci | ?{ $_.Name -match "^.*\.symbols\.nupkg" } | %{ $_.Name } | %{ . $nuget "Push" $_ "-Source" $symbolServer }