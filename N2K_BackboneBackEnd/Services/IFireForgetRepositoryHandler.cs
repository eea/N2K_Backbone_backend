using NuGet.Protocol.Core.Types;
using System;
using System.Threading.Tasks;

namespace N2K_BackboneBackEnd.Services
{
    public interface IFireForgetRepositoryHandler
    {
        void Execute(Func<IHarvestedService, Task> databaseWork);
    }
}


