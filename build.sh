#!/usr/bin/env sh

wget tools/nuget.exe http://nuget.org/nuget.exe

mono --runtime=v4.0 tools/NuGet/nuget.exe install FAKE -OutputDirectory tools -ExcludeVersion
mono --runtime=v4.0 tools/NuGet/nuget.exe install nunit.runner -OutputDirectory tools -ExcludeVersion 
mono --runtime=v4.0 tools/FAKE/tools/FAKE.exe build.fsx $@
