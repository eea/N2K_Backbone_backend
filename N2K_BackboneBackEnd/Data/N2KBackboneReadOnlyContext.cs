using N2K_BackboneBackEnd.Models;

using N2K_BackboneBackEnd.Models.BackboneDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;


using System.Reflection;
using N2K_BackboneBackEnd.Models.backbone_db;

namespace N2K_BackboneBackEnd.Data
{
    public class N2KBackboneReadOnlyContext : BaseContext
    {

        public N2KBackboneReadOnlyContext(DbContextOptions<N2KBackboneReadOnlyContext> options) : base(options) { }

    }
}