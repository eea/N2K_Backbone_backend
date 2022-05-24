using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.ViewModel
{

    [Keyless]
    public class CategoryChangeDetail : IEntityModel
    {
        public string ChangeType { get; set; } = "";
        public string ChangeCategory { get; set; } = "";

        public string FieldName { get; set; } = "";

        [NotMapped]
        public List<CodeChangeDetail> ChangedCodes { get; set; } = new List<CodeChangeDetail>();
        [NotMapped]
        public List<CodeAddedDetail> AddedCodes { get; set; } = new List<CodeAddedDetail>();


        public CategoryChangeDetail()
        {
            ChangedCodes = new List<CodeChangeDetail>();
            AddedCodes = new List<CodeAddedDetail>();
        }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CategoryChangeDetail>();
        }

    }
}
