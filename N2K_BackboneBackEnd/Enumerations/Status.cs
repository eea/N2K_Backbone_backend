using System.Runtime.Serialization;

namespace N2K_BackboneBackEnd.Enumerations
{
    [DataContract]
    public enum SiteChangeStatus
    {
        [DataMember]
        Accepted,
        [DataMember]
        Pending,
        [DataMember]
        Rejected
    }
}