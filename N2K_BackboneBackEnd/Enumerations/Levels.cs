using System.Runtime.Serialization;

namespace N2K_BackboneBackEnd.Enumerations
{
    [DataContract]
    public enum Level
    {
        [DataMember]
        Warning,
        [DataMember]
        Medium,
        [DataMember]
        Critical
    }
}
