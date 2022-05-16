using System.Runtime.Serialization;

namespace N2K_BackboneBackEnd.Enumerations
{
    [DataContract]
    public enum Level
    {
        [DataMember]
        Info,
        [DataMember]
        Warning,
        [DataMember]
        Critical
    }
}
