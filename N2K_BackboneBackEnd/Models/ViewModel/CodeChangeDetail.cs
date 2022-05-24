using Microsoft.EntityFrameworkCore;


namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class CodeChangeDetail : IEntityModel
    {
        public string? Code { get; set; } = "";
        public long ChangeId { get; set; }
        public string? ReportedValue { get; set; } = "";
        public string? OlValue { get; set; } = "";

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CodeChangeDetail>();
        }
    }
}
