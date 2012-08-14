using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Meulekamp.NuGetReferenceChecker.Model
{
    // ReSharper disable InconsistentNaming
    // supress reason: mostly model classes which reflect entities used in xml files.

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DebuggerDisplay("path={path}")]
    public class repository
    {
        /// <remarks/>
        [XmlAttribute]
        public string path { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DebuggerDisplay("NuGetPackage={id}.{version}")]
    public class package
    {
        /// <remarks/>
        [XmlAttribute]
        public string id { get; set; }

        /// <remarks/>
        [XmlAttribute]
        public string version { get; set; }
    }
        
    [SerializableAttribute]
    [XmlTypeAttribute(AnonymousType = true)]
    [XmlRootAttribute(Namespace = "", IsNullable = false)]
    [DebuggerDisplay("Reference={Include} -> HintPath={HintPath}")]
    public class Reference
    {
        /// <remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string HintPath { get; set; }

        /// <remarks/>
        [XmlAttribute]
        public string Include { get; set; }
    }
    // ReSharper restore InconsistentNaming
}