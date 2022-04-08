using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models
{
    public interface IEntityModel
    {
        void  OnModelCreating(ModelBuilder builder);
    }
}