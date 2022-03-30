using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace N2K_BackboneBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteChangesController : ControllerBase
    {
        // GET: api/<SiteChangesController>
        [HttpGet]
        public IEnumerable<SiteChange> Get()
        {
            return new SiteChange[] { 
                new SiteChange {
                     SiteCode= "ES0000153",
                     Country= "Spain",
                     Status= "Accepted",
                     Tags = null,
                     ChangeId= 1,
                     Level="Critical",
                     ChangeCategory="Area decrease",
                     ChangeType="AreaHa"
                },
                new SiteChange {
                     SiteCode= "ES0000546",
                     Country= "Spain",
                     Status= "Accepted",
                     Tags= null,
                     ChangeId= 2,
                     Level= "Warning",
                     ChangeCategory= "Site name change",
                     ChangeType= "Sitename"

                },
                new SiteChange {
                     SiteCode= "ES0000390",
                     Country= "Spain",
                     Status= "Rejected",
                     Tags= null,
                     ChangeId= 3,
                     Level= "Warning",
                     ChangeCategory= "Site length change",
                     ChangeType= "Length (Km)"
                },
                new SiteChange {
                     SiteCode= "ES0000153",
                     Country= "Spain",
                     Status= "Accepted",
                     Tags= null,
                     ChangeId= 4,
                     Level= "Critical",
                     ChangeCategory= "Priority deleted",
                     ChangeType= "Priority (A, B, C...)"

                },
                new SiteChange {
                     SiteCode= "AT1206A00",
                     Country= "Austria",
                     Status= "Pending",
                     Tags= null,
                     ChangeId= 5,
                     Level= "Critical",
                     ChangeCategory= "Cover decrease",
                     ChangeType= "Cover_ha"
                },
                new SiteChange {
                     SiteCode= "AT1206A00",
                     Country= "Austria",
                     Status= "Pending",
                     Tags= null,
                     ChangeId= 6,
                     Level= "Warning",
                     ChangeCategory= "Centroid change",
                     ChangeType= "Latitude-Longitude (centroid)"
                },
                new SiteChange {
                     SiteCode= "ES6150012",
                     Country= "Spain",
                     Status= "Pending",
                     Tags= null,
                     ChangeId= 7,
                     Level= "Warning",
                     ChangeCategory= "Site name change",
                     ChangeType= "Sitename"
                },
                new SiteChange {
                     SiteCode= "ES6150012",
                     Country= "Spain",
                     Status= "Pending",
                     Tags= null,
                     ChangeId= 8,
                     Level= "Warning",
                     ChangeCategory= "Site length change",
                     ChangeType= "Length (Km)"
                },
                new SiteChange {
                     SiteCode= "ES6150012",
                     Country= "Spain",
                     Status= "Pending",
                     Tags= null,
                     ChangeId= 9,
                     Level= "Medium",
                     ChangeCategory= "Area decrease",
                     ChangeType= "AreaHa"
                },
                new SiteChange {
                     SiteCode= "ES6150012",
                     Country= "Spain",
                     Status= "Pending",
                     Tags= null,
                     ChangeId= 10,
                     Level= "Critical",
                     ChangeCategory= "Priority deleted",
                     ChangeType= "Priority (A, B, C...)"
                },
                new SiteChange {
                     SiteCode= "HR3000198",
                     Country= "Croatia",
                     Status= "Accepted",
                     Tags= null,
                     ChangeId= 11,
                     Level= "Critical",
                     ChangeCategory= "Priority deleted",
                     ChangeType= "Priority (A, B, C...)"
                },
                new SiteChange {
                     SiteCode= "HR3000199",
                     Country= "Croatia",
                     Status= "Accepted",
                     Tags= null,
                     ChangeId= 12,
                     Level= "Critical",
                     ChangeCategory= "Area decrease",
                     ChangeType= "AreaHa"
                },
                new SiteChange {
                     SiteCode= "FR4301287",
                     Country= "France",
                     Status= "Rejected",
                     Tags= null,
                     ChangeId= 13,
                     Level= "Warning",
                     ChangeCategory= "Site name change",
                     ChangeType= "Sitename"
                }

            };
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
