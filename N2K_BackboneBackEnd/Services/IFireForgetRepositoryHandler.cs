namespace N2K_BackboneBackEnd.Services
{
    public interface IFireForgetRepositoryHandler
    {
        void Execute(Func<IHarvestedService, Task> databaseWork);
    }
}