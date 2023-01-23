using System.Runtime.Serialization;

namespace N2K_BackboneBackEnd.Enumerations
{
    [DataContract]
    public enum SiteChangeStatus
    {
        [DataMember]
        Pending = 0,
        [DataMember]
        Accepted = 1,
        [DataMember]
        Rejected = 2,
        [DataMember]
        Harvested = 3,
        [DataMember]
        Harvesting = 4,
        [DataMember]
        Queued = 5,
        [DataMember]
        PreHarvested = 6,
        [DataMember]
        Discarded = 7,
        [DataMember]
        Closed = 8,
        [DataMember]
        DataLoaded = 9,
        [DataMember]
        Error = 10

    }
}