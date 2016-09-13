using System.Web.Http;

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
        public string Get(int? id, string x, DummyClass d)
        {
            return $"value {id}";
        }

        [HttpGet]
        [Route("")]
        public string Getty(string x, int y)
        {
            return $"value {x} {y}";
        }
    }
}