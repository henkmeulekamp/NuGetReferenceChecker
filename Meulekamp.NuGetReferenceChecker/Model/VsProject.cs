using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Meulekamp.NuGetReferenceChecker.Model
{
    [Serializable]
    [DebuggerDisplay("Project={ProjectFile}")]
    public class VsProject
    {
        public string ProjectPath;
        public string ProjectFile;
        public List<package> ListPackages;
        public string PackageFile;
    }
}