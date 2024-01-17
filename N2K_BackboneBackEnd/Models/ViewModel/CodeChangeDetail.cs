using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class CodeChangeDetail : IEntityModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public long ChangeId { get; set; }
        public Dictionary<string, string> Fields { get; set; } = new Dictionary<string, string>();

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CodeChangeDetail>();
        }
    }


    [Keyless]
    public class CodeChangeDetailModify : CodeChangeDetail
    {
        public string? Reported { get; set; } = "";
        public string? Reference { get; set; } = "";

        new public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CodeChangeDetailModify>();
        }
    }

    [Keyless]
    public class CodeChangeDetailAddedRemovedSpecies : CodeChangeDetail
    {
        public string? AnnexII { get; set; } = "";
        public string? Priority { get; set; } = "";
        public string? Population { get; set; } = "";
        public string? SpeciesType { get; set; } = "";

        new public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CodeChangeDetailAddedRemovedSpecies>();
        }
    }

    [Keyless]
    public class CodeChangeDetailAddedRemovedHabitats : CodeChangeDetail
    {
        public string? Priority { get; set; } = "";
        public string? CoverHa { get; set; } = "";
        public string? RelSurface { get; set; } = "";

        new public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CodeChangeDetailAddedRemovedHabitats>();
        }
    }
}