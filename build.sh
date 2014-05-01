#!/usr/bin/env sh

mkdir tools
cd tools
wget http://nuget.org/nuget.exe
cd ..

mono --runtime=v4.0 tools/nuget.exe install FAKE -OutputDirectory tools -ExcludeVersion
mono --runtime=v4.0 tools/nuget.exe install nunit.runner -OutputDirectory tools -ExcludeVersion 
mono --runtime=v4.0 tools/FAKE/tools/FAKE.exe build.fsx $@
