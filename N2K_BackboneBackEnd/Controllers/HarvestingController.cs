using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace N2K_BackboneBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HarvestingController : ControllerBase
    {
        // GET: api/<HarvestingController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "valueTwo" };
        }


        [HttpGet]
        [Route("HarvestedAA")]
        public IEnumerable<string> HarvestedAA()
        {
            return new string[] { "value1", "value2", "VALUE3" };
        }

        [HttpGet]
        [Route("Harvested")]
        public IEnumerable<string> Harvested()
        {
            return new string[] { "value1", "value2" };
        }
        
        
        [HttpGet("Harvested/{fromDate}")]
        public IEnumerable<string> Harvested(DateTime? fromDate)
        {
            if (!fromDate.HasValue) return Harvested();
            return new string[] { "value1", "value2" };
        }

        [HttpGet("Harvested/{fromDate}/{toDate}")]
        public IEnumerable<string> Harvested(DateTime fromDate, DateTime? toDate)
        {
            return new string[] { "value1", "value2" };
        }


        [HttpGet]
        [Route("Pending")]
        public IEnumerable<string> Pending()
        {
            return new string[] { "value1", "value2" };
        }


        [HttpGet]
        [Route("PendingByCountry/{CountryCode}")]
        public IEnumerable<string> PendingByCountry(string CountryCode)
        {
            return new string[] { "value1", "value2" };
        }


        // GET api/<HarvestingController>/5
        //Id=>envelopeID
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<HarvestingController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<HarvestingController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<HarvestingController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
