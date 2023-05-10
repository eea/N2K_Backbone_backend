using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models
{
    public interface IEntityModel
    {
        static void  OnModelCreating(ModelBuilder builder) { }
    }

    public interface IEntityModelBackboneDB { }

    public interface IEntityModelBackboneDBHarvesting
    {
        public void SaveRecord(string db);
    }

    public interface IEntityModelVersioningDB { }

    public interface IEntityModelReleasesDB { }

    public interface IEntityModelBackboneReadOnlyDB { }
}