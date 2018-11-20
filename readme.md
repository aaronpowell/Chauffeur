# Chauffeur

<img src="./chauffeur_logo.svg" width="50" />

| Build type | Status | NuGet | Code Coverage |
| --- | --- | --- | --- |
| master | [![Build Status](https://aaronpowell.visualstudio.com/Chauffeur/_apis/build/status/Chauffeur%20Pipeline?branchName=master)](https://aaronpowell.visualstudio.com/Chauffeur/_build/latest?definitionId=14) | [![NuGet Badge](https://buildstats.info/nuget/Chauffeur)](https://www.nuget.org/packages/Chauffeur/) | [![codecov](https://codecov.io/gh/aaronpowell/chauffeur/branch/master/graph/badge.svg)](https://codecov.io/gh/aaronpowell/chauffeur)
| dev | [![Build Status](https://aaronpowell.visualstudio.com/Chauffeur/_apis/build/status/Chauffeur%20Pipeline)](https://aaronpowell.visualstudio.com/Chauffeur/_build/latest?definitionId=14) | [![NuGet Badge](https://buildstats.info/nuget/Chauffeur?includePreReleases=true)](https://www.nuget.org/packages/Chauffeur/) |  |


Welcome to Chauffeur, deliverying changes to your Umbraco environment in style.

## Who is Chauffeur?

Chauffeur is a CLI for Umbraco, it will sit with your Umbraco websites `bin` folder and give you an interface to which you can execute commands, known as **Deliverables**, against your installed Umbraco instance.

Chauffeur is developed for Umbraco 7.x as it is designed around the new Umbraco API.

# Getting Started

To get started install Chauffeur and open up a command window and launch `Chauffeur.Runner.exe`, which is the entry point for Chauffeur. From the prompt you can get started running deliverables.

# Running Demo

The easiest way to run the demo application is to execute the `run-demo.ps1` script from the root. This will:

- Compile
- Install the demo website using Chauffeur
- Start IIS Express for the demo website

# License

Chauffeur is licensed under [MIT](License.md).

Chauffeur Logo is from [Ed Piel](https://thenounproject.com/eduardpiel) used under Creative Commons from [The Noun Project](https://thenounproject.com/term/chauffeur/239487)