using System.Runtime.Serialization;


namespace N2K_BackboneBackEnd.Enumerations
{
    [DataContract]
    public enum HarvestingStatus
    {
        [DataMember]
        Pending,
        [DataMember]
        Harvesting,
        [DataMember]
        Completed
    }

}
