#!/usr/bin/env sh

mono --runtime=v4.0 tools/nuget.exe install FAKE -OutputDirectory tools -ExcludeVersion
mono --runtime=v4.0 tools/nuget.exe install nunit.runners -OutputDirectory tools -ExcludeVersion 
mono --runtime=v4.0 tools/FAKE/tools/FAKE.exe build.fsx $@
