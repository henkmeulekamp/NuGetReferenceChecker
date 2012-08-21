NuGetReferenceChecker
=====================

Csharp nuget reference checker.

Small library which will test a visualstudio solution for some basic problems in nuget package usage. 

It currently has 5 tests:

* FindWronglyReferencedNugetAssemblies.
This will find all references from csproj files where the assembly file itself is in one of the nuget packages and the project reference hint path is hinting to any other folder then <SolutionRoot>/packages. 

* FindPackageFilesNotInRepositoryFile.
Finds all packages.config which are not mentioned in the  <SolutionRoot>/packages/repositories.config

* FindMissingPackages.
Looks for nuget packages mentioned in packages.config files and which are not found in  <SolutionRoot>/packages

* FindAllPackagesWithMultipleVersions.
Finds all nuget packages with multiple versions referenced. (Project A linking version 1.0.1 and Project B linking to version 1.0.2 of the same nuget package)

* FindAllNonUsedPackages.
Finds all package folders in  <SolutionRoot>/packages which are not referenced by any repositories.config. These can be deleted.  

Running these tests makes some assumptions about your solution setup, the library is currently limited to:  
- C#, .NET 4.0 and 4.5 (Tested in VS2010 and VS2012)  
- Solution folder as root where projects are somewhere in its folder tree.  
  
If the tests are run without giving a solution directory as parameter it will go 4 levels up the tree to search for the solution root and identifies this by seeing the .sln file in the root.

See [github project download page](https://github.com/henkmeulekamp/NuGetReferenceChecker/downloads) for latest binary.

# More info
_Some more info can be found [here](http://www.serverside-developer.com/2012/08/nuget-referencechecker-to-check-invalid.html)._
