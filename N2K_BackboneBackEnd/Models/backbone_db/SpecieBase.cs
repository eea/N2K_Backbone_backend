namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SpecieBase
    {
        public long id { get; set; }
        public string SiteCode { get; set; } = "";
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
        public bool? Other { get; set; }

        public Species getSpecies()
        {
            Species specie = new()
            {
                SiteCode = this.SiteCode,
                Version = this.Version,
                SpecieCode = this.SpecieCode,
                PopulationMin = this.PopulationMin,
                PopulationMax = this.PopulationMax,
                //item.Group = element.GROUP; // PENDING
                SensitiveInfo = this.SensitiveInfo,
                Resident = this.Resident,
                Breeding = this.Breeding,
                Winter = this.Winter,
                Staging = this.Staging,
                //item.Path = element.PATH; // ??? PENDING
                AbundaceCategory = this.AbundaceCategory,
                Motivation = this.Motivation,
                PopulationType = this.PopulationType,
                CountingUnit = this.CountingUnit,
                Population = this.Population,
                Insolation = this.Insolation,
                Conservation = this.Conservation,
                Global = this.Global,
                NonPersistence = this.NonPersistence,
                DataQuality = this.DataQuality,
                SpecieType = this.SpecieType
            };

            return specie;
        }

        public SpeciesOther getSpeciesOther()
        {
            SpeciesOther specie = new()
            {
                SiteCode = this.SiteCode,
                Version = this.Version,
                SpecieCode = this.SpecieCode,
                PopulationMin = this.PopulationMin,
                PopulationMax = this.PopulationMax,
                //item.Group = element.GROUP; // PENDING
                SensitiveInfo = this.SensitiveInfo,
                Resident = this.Resident,
                Breeding = this.Breeding,
                Winter = this.Winter,
                Staging = this.Staging,
                //item.Path = element.PATH; // ??? PENDING
                AbundaceCategory = this.AbundaceCategory,
                Motivation = this.Motivation,
                PopulationType = this.PopulationType,
                CountingUnit = this.CountingUnit,
                Population = this.Population,
                Insolation = this.Insolation,
                Conservation = this.Conservation,
                Global = this.Global,
                NonPersistence = this.NonPersistence,
                DataQuality = this.DataQuality,
                SpecieType = this.SpecieType
            };

            return specie;
        }
    }
}