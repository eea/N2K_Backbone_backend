using System.Runtime.Serialization;

namespace N2K_BackboneBackEnd.Enumerations
{
    [DataContract]
    public enum LineageTypes
    {
        [DataMember]
        NoChanges = 0,
        [DataMember]
        Creation = 1,
        [DataMember]
        Deletion = 2,
        [DataMember]
        Split = 3,
        [DataMember]
        Merge = 4,
        [DataMember]
        Recode = 5,
        [DataMember]
        NewGeometryReported = 6,
        [DataMember]
        NoGeometryReported = 7

    }
}