using System.Web.Http;

namespace WebApiTestApplication.Controllers
{
    [RoutePrefix("api/thingy")]
    public class ThingyController : ApiController
    {
        [HttpGet]
        [Route("")]
        public string Get(int? id, string x, DummyClass d)
        {
            return $"value {id}";
        }
    }
}