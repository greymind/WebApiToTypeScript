using System.Web.Http;
using Newtonsoft.Json;

namespace WebApiTestApplication.Controllers
{
    [RoutePrefix("api/thingy")]
    public class ThingyController : ApiController
    {
        [HttpGet]
        [Route("")]
        public string GetAll()
        {
            return $"values";
        }

        [HttpGet]
        [Route("{id}")]
        public string Get(int? id, string x, [FromUri]AnotherClass c)
        {
            var valueJson = JsonConvert.SerializeObject(c);
            return $"value {id} {x} {valueJson}";
        }

        [HttpGet]
        [Route("")]
        public string Getty(string x, int y)
        {
            return $"value {x} {y}";
        }
    }
}