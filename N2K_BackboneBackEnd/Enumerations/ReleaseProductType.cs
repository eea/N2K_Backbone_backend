using System.Runtime.Serialization;

namespace N2K_BackboneBackEnd.Enumerations
{
    [DataContract]
    public enum ReleaseProductType
    {
        [DataMember]
        SHP = 0,
        [DataMember]
        MDB_Official = 1,
        [DataMember]
        MDB_Public = 2,
        [DataMember]
        GPKG_Official = 3,
        [DataMember]
        GPKG_Public = 4
    }
}
