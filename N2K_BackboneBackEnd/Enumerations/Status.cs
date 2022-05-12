using System.Runtime.Serialization;

namespace N2K_BackboneBackEnd.Enumerations
{
    [DataContract]
    public enum SiteChangeStatus
    {
        [DataMember]
        Pending = 0,
        [DataMember]
        Accepted=1,
        [DataMember]
        Rejected=2,
        [DataMember]
        Harvested=3

    }
}