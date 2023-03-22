using System.Runtime.Serialization;

namespace N2K_BackboneBackEnd.Enumerations
{
    [DataContract]
    public enum Level
    {
        [DataMember]
        Info = 0,
        [DataMember]
        Warning = 1,
        [DataMember]
        Critical= 2
    }
}
