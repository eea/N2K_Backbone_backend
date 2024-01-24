namespace N2K_BackboneBackEnd.Models.backbone_db
{
    /// <summary>
    /// Class to store both list of species (Oficial and unoffical. It allows to travel the whole list accross the classes
    /// </summary>
    public class SiteSpecies
    {
        public List<Species> CatalogedList { get; set; } = new List<Species>();
        public List<SpeciesOther> UncatalogedList { get; set; } = new List<SpeciesOther>();
    }
}