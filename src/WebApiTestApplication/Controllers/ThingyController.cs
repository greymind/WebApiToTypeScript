using System.Web.Http;
using Newtonsoft.Json;

namespace WebApiTestApplication.Controllers
{
    public class MegaClass : AnotherClass
    {
        public int Something { get; set; }
    }

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
        public string Get(int? id, string x, [FromUri]MegaClass c)
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

        [HttpPost]
        [Route("")]
        public string Post(MegaClass value)
        {
            var valueJson = JsonConvert.SerializeObject(value);
            return $"thanks for the {valueJson} in the ace!";
        }
    }
}