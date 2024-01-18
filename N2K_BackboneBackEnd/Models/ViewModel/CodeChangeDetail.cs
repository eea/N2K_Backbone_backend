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
        public string? Reported { get; set; } = string.Empty;
        public string? Reference { get; set; } = string.Empty;

        new public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CodeChangeDetailModify>();
        }
    }

    [Keyless]
    public class CodeChangeDetailAddedRemovedSpecies : CodeChangeDetail
    {
        public string? AnnexII { get; set; } = string.Empty;
        public string? Priority { get; set; } = string.Empty;
        public string? Population { get; set; } = string.Empty;
        public string? SpeciesType { get; set; } = string.Empty;

        new public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CodeChangeDetailAddedRemovedSpecies>();
        }
    }

    [Keyless]
    public class CodeChangeDetailAddedRemovedHabitats : CodeChangeDetail
    {
        public string? Priority { get; set; } = string.Empty;
        public string? CoverHa { get; set; } = string.Empty;
        public string? RelSurface { get; set; } = string.Empty;

        new public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CodeChangeDetailAddedRemovedHabitats>();
        }
    }
}