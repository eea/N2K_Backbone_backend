using System.Runtime.Serialization;

namespace N2K_BackboneBackEnd.Enumerations
{
    [DataContract]
    public enum LineageStatus
    {
        [DataMember]
        Proposed = 0,
        [DataMember]
        Consolidated = 1
    }
}