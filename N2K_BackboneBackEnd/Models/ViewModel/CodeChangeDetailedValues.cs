using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class CodeChangeDetailedValues : IEntityModel
    {
        public string? Name { get; set; }
        public string? Value { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CodeChangeDetailedValues>();
        }

    }
}
