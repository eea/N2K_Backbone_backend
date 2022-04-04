using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;

namespace N2K_BackboneBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteChangesController : ControllerBase
    {
        private readonly N2KBackboneContext _context;

        public SiteChangesController(N2KBackboneContext context)
        {
            _context = context;
        }


        [Route("GetAsync")]
        [HttpGet]        
        public async Task<ActionResult<IEnumerable<SiteChange>>> GetAsync()
        {
            return await _context.SiteChanges.ToListAsync();
        }


        [Route("Get")]
        [HttpGet]
        public IEnumerable<SiteChange> Get()
        {
            var _list = _context.SiteChanges.ToList();
            //return _list.OrderBy(a => a.ChangeId).ToList();
            return _list;
        }
        

        // GET api/<SiteChangesController>/5
        [HttpGet("{id}")]
        public SiteChange Get(int id)
        {
            return new SiteChange { ChangeCategory = "Site name change", ChangeId = 1, Country = "ES", Level = "Warning", ChangeType = "Sitename", SiteCode = "ES0000153", Status = "Accepted", Tags = null };
        }

        /*
        // POST api/<SiteChangesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<SiteChangesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<SiteChangesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
        */
    }
}
