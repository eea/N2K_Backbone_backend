namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SpecieBase
    {
        public long id { get; set; }
        public string SiteCode { get; set; }
        public int Version { get; set; }
        public string? SpecieCode { get; set; }
        public int? PopulationMin { get; set; }
        public int? PopulationMax { get; set; }
        public string? Group { get; set; }
        public Boolean? SensitiveInfo { get; set; }
        public string? Resident { get; set; }
        public string? Breeding { get; set; }
        public string? Winter { get; set; }
        public string? Staging { get; set; }
        public string? Path { get; set; }
        public string? AbundaceCategory { get; set; }
        public string? Motivation { get; set; }
        public string? PopulationType { get; set; }
        public string? CountingUnit { get; set; }
        public string? Population { get; set; }
        public string? Insolation { get; set; }
        public string? Conservation { get; set; }
        public string? Global { get; set; }
        public Boolean? NonPersistence { get; set; }
        public string? DataQuality { get; set; }
        public string? SpecieType { get; set; }

        public Species getSpecies() { 
            Species specie = new Species();
            specie.SiteCode = this.SiteCode;
            specie.Version = this.Version;
            specie.SpecieCode = this.SpecieCode;
            specie.PopulationMin = this.PopulationMin;
            specie.PopulationMax = this.PopulationMax;
            //item.Group = element.GROUP; // PENDING
            specie.SensitiveInfo = this.SensitiveInfo;
            specie.Resident = this.Resident;
            specie.Breeding = this.Breeding;
            specie.Winter = this.Winter;
            specie.Staging = this.Staging;
            //item.Path = element.PATH; // ??? PENDING
            specie.AbundaceCategory = this.AbundaceCategory;
            specie.Motivation = this.Motivation;
            specie.PopulationType = this.PopulationType;
            specie.CountingUnit = this.CountingUnit;
            specie.Population = this.Population;
            specie.Insolation = this.Insolation;
            specie.Conservation = this.Conservation;
            specie.Global = this.Global;
            specie.NonPersistence = this.NonPersistence;
            specie.DataQuality = this.DataQuality;
            specie.SpecieType = this.SpecieType;

            return specie;
        }
        public SpeciesOther getSpeciesOther()
        {
            SpeciesOther specie = new SpeciesOther();
            specie.SiteCode = this.SiteCode;
            specie.Version = this.Version;
            specie.SpecieCode = this.SpecieCode;
            specie.PopulationMin = this.PopulationMin;
            specie.PopulationMax = this.PopulationMax;
            //item.Group = element.GROUP; // PENDING
            specie.SensitiveInfo = this.SensitiveInfo;
            specie.Resident = this.Resident;
            specie.Breeding = this.Breeding;
            specie.Winter = this.Winter;
            specie.Staging = this.Staging;
            //item.Path = element.PATH; // ??? PENDING
            specie.AbundaceCategory = this.AbundaceCategory;
            specie.Motivation = this.Motivation;
            specie.PopulationType = this.PopulationType;
            specie.CountingUnit = this.CountingUnit;
            specie.Population = this.Population;
            specie.Insolation = this.Insolation;
            specie.Conservation = this.Conservation;
            specie.Global = this.Global;
            specie.NonPersistence = this.NonPersistence;
            specie.DataQuality = this.DataQuality;
            specie.SpecieType = this.SpecieType;

            return specie;
        }

    }
}
