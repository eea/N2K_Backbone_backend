using N2K_BackboneBackEnd.Data;

namespace N2K_BackboneBackEnd.Services
{
    public class LongRunningService: ILongRunningService
    {

        private readonly N2KBackboneContext _dataContext;


        public async Task<int> TestLongRun()
        {
            await Task.Delay(20000);
            return 1;
        }
    }
}
