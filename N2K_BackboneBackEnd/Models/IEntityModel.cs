using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models
{
    public interface IEntityModel
    {
        static void  OnModelCreating(ModelBuilder builder) { }
    }
}